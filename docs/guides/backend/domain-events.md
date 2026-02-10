# Domain Events

**Purpose:** Guide for implementing domain events with Wolverine in Family Hub.

**Pattern:** Aggregates raise events via `RaiseDomainEvent()`. Events are published by `AppDbContext.SaveChangesAsync()` after successful persistence. Wolverine auto-discovers static event handler classes.

---

## Event Flow

```
1. Aggregate factory/method calls RaiseDomainEvent(new SomeEvent(...))
       |
       v
2. Events stored in aggregate's DomainEvents collection
       |
       v
3. Repository calls SaveChangesAsync()
       |
       v
4. AppDbContext.SaveChangesAsync() override:
   a. Collects all DomainEvents from tracked aggregates
   b. Persists changes to database
   c. Publishes each event via Wolverine IMessageBus.PublishAsync()
   d. Clears events from aggregates via ClearDomainEvents()
       |
       v
5. Wolverine discovers and invokes matching event handler
```

---

## Defining Events

Events are `sealed record` types that extend the `DomainEvent` base record:

```csharp
// src/FamilyHub.Api/Features/Family/Domain/Events/InvitationSentEvent.cs

public sealed record InvitationSentEvent(
    InvitationId InvitationId,
    FamilyId FamilyId,
    UserId InvitedByUserId,
    Email InviteeEmail,
    FamilyRole Role,
    string PlaintextToken,
    DateTime ExpiresAt
) : DomainEvent;
```

The base class provides automatic `EventId` and `OccurredAt`:

```csharp
// src/FamilyHub.Api/Common/Domain/DomainEvent.cs

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
```

The marker interface:

```csharp
// src/FamilyHub.Api/Common/Domain/IDomainEvent.cs

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
```

### Event Naming Convention

Events are named in past tense, describing what happened:

| Event | Trigger |
|-------|---------|
| `FamilyCreatedEvent` | Family aggregate created |
| `InvitationSentEvent` | Invitation aggregate created |
| `InvitationAcceptedEvent` | Invitation accepted |
| `InvitationDeclinedEvent` | Invitation declined |
| `InvitationRevokedEvent` | Invitation revoked |
| `UserRegisteredEvent` | User registered |
| `UserFamilyAssignedEvent` | User assigned to a family |
| `UserFamilyRemovedEvent` | User removed from a family |

### Event Location

Events are defined in the domain layer:

```
src/FamilyHub.Api/Features/{Module}/Domain/Events/
  {EventName}.cs
```

---

## Raising Events in Aggregates

Events are raised inside aggregate factory methods and state-changing methods using the inherited `RaiseDomainEvent()`:

```csharp
// src/FamilyHub.Api/Features/Family/Domain/Entities/FamilyInvitation.cs

public sealed class FamilyInvitation : AggregateRoot<InvitationId>
{
    public static FamilyInvitation Create(
        FamilyId familyId,
        UserId invitedByUserId,
        Email inviteeEmail,
        FamilyRole role,
        InvitationToken tokenHash,
        string plaintextToken)
    {
        var invitation = new FamilyInvitation
        {
            Id = InvitationId.New(),
            FamilyId = familyId,
            // ... set properties
        };

        // Raise event in factory method
        invitation.RaiseDomainEvent(new InvitationSentEvent(
            invitation.Id,
            invitation.FamilyId,
            invitation.InvitedByUserId,
            invitation.InviteeEmail,
            invitation.Role,
            plaintextToken,
            invitation.ExpiresAt
        ));

        return invitation;
    }

    public void Accept(UserId userId)
    {
        // ... validate state

        Status = InvitationStatus.Accepted;
        AcceptedByUserId = userId;
        AcceptedAt = DateTime.UtcNow;

        // Raise event in state-changing method
        RaiseDomainEvent(new InvitationAcceptedEvent(
            Id, FamilyId, userId, Role));
    }
}
```

### AggregateRoot Base Class

```csharp
// src/FamilyHub.Api/Common/Domain/AggregateRoot.cs

public abstract class AggregateRoot<TId> where TId : struct
{
    public TId Id { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

---

## Event Handlers

Event handlers are **static classes** with a `Handle()` method, following the same Wolverine convention as command handlers:

```csharp
// src/FamilyHub.Api/Features/Family/Application/EventHandlers/InvitationSentEventHandler.cs

public static class InvitationSentEventHandler
{
    public static async Task Handle(
        InvitationSentEvent @event,          // The event (first parameter)
        IEmailService emailService,           // Injected by Wolverine
        IFamilyRepository familyRepository,
        IUserRepository userRepository,
        IConfiguration configuration,
        CancellationToken ct)
    {
        var family = await familyRepository.GetByIdAsync(@event.FamilyId, ct);
        var inviter = await userRepository.GetByIdAsync(@event.InvitedByUserId, ct);

        var familyName = family?.Name.Value ?? "Unknown Family";
        var inviterName = inviter?.Name.Value ?? "A family member";

        var frontendUrl = configuration["App:FrontendUrl"] ?? "http://localhost:4200";
        var acceptUrl = $"{frontendUrl}/invitation/accept?token=" +
            Uri.EscapeDataString(@event.PlaintextToken);

        var htmlBody = InvitationEmailTemplate.GenerateHtml(
            familyName, inviterName, @event.Role.Value, acceptUrl, @event.ExpiresAt);

        await emailService.SendEmailAsync(
            @event.InviteeEmail.Value,
            $"You've been invited to join {familyName} on Family Hub",
            htmlBody, textBody, ct);
    }
}
```

### Handler Location

Event handlers are in the `Application/EventHandlers/` folder:

```
src/FamilyHub.Api/Features/{Module}/Application/EventHandlers/
  {EventName}Handler.cs
```

### Handler Discovery

Wolverine auto-discovers event handlers by convention:

- Class name: `{EventName}Handler`
- Method name: `Handle`
- First parameter type matches the event record type

No manual registration is needed.

---

## Event Publishing Mechanism

The `AppDbContext` overrides `SaveChangesAsync` to collect and publish domain events:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    // 1. Collect events from all aggregate roots
    var aggregateEntries = ChangeTracker.Entries()
        .Where(e => e.Entity.GetType().BaseType?.IsGenericType == true &&
                     e.Entity.GetType().BaseType.GetGenericTypeDefinition()
                         == typeof(AggregateRoot<>))
        .ToList();

    var domainEvents = new List<IDomainEvent>();
    foreach (var entry in aggregateEntries)
    {
        var events = entry.Entity.GetType()
            .GetProperty("DomainEvents")
            ?.GetValue(entry.Entity) as IEnumerable<IDomainEvent>;
        if (events != null) domainEvents.AddRange(events);
    }

    // 2. Save to database
    var result = await base.SaveChangesAsync(cancellationToken);

    // 3. Publish events after successful save
    if (_messageBus is not null)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _messageBus.PublishAsync(domainEvent);
        }
    }

    // 4. Clear events from aggregates
    foreach (var entry in aggregateEntries)
    {
        entry.Entity.GetType()
            .GetMethod("ClearDomainEvents")
            ?.Invoke(entry.Entity, null);
    }

    return result;
}
```

This uses reflection because `ChangeTracker` entries are of type `object`. The events are published after the database transaction succeeds, ensuring consistency.

---

## Cross-Module Events

Events can cross module boundaries. For example, `UserFamilyAssignedEvent` is raised in the Auth module but handled by event handlers that might live in the Family module:

```
Auth module: User.AssignToFamily() raises UserFamilyAssignedEvent
Family module: (future) could handle this event for family-specific logic
```

Wolverine handles cross-module event routing automatically since all handlers are in the same assembly.

---

## Checklist: Adding a New Domain Event

1. Define the event record in `Domain/Events/`:

   ```csharp
   public sealed record MyEvent(
       MyId Id,
       // ... relevant data
   ) : DomainEvent;
   ```

2. Raise the event in the aggregate:

   ```csharp
   RaiseDomainEvent(new MyEvent(Id, ...));
   ```

3. Create the handler in `Application/EventHandlers/`:

   ```csharp
   public static class MyEventHandler
   {
       public static async Task Handle(
           MyEvent @event,
           // ... injected dependencies
           CancellationToken ct)
       {
           // Handle the event
       }
   }
   ```

4. No registration needed. Wolverine discovers the handler automatically.

---

## Testing Domain Events

### Test that events are raised

```csharp
[Fact]
public void Create_ShouldRaiseInvitationSentEvent()
{
    var invitation = FamilyInvitation.Create(
        TestFamilyId, TestInviterId, TestEmail, TestRole,
        InvitationToken.From(TestTokenHash), "plaintext-token");

    invitation.DomainEvents.Should().HaveCount(1);
    var domainEvent = invitation.DomainEvents.First();
    domainEvent.Should().BeOfType<InvitationSentEvent>();

    var sentEvent = (InvitationSentEvent)domainEvent;
    sentEvent.FamilyId.Should().Be(TestFamilyId);
}
```

### Clear events between operations

```csharp
[Fact]
public void Accept_ShouldRaiseInvitationAcceptedEvent()
{
    var invitation = CreateTestInvitation();
    invitation.ClearDomainEvents();    // Clear the Create event

    invitation.Accept(UserId.New());

    invitation.DomainEvents.Should().HaveCount(1);
    invitation.DomainEvents.First().Should().BeOfType<InvitationAcceptedEvent>();
}
```

---

## Related Guides

- [Handler Patterns](handler-patterns.md) -- command handlers that trigger domain events
- [EF Core Patterns](ef-core-patterns.md) -- SaveChangesAsync publishes events
- [Testing Patterns](testing-patterns.md) -- testing event raising and handlers
- [Vogen Value Objects](vogen-value-objects.md) -- value objects used in events

---

**Last Updated:** 2026-02-09
