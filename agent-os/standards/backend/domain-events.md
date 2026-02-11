# Domain Events

Events extend `DomainEvent` base record. Handlers are Wolverine static classes with `Handle()` method.

## Event Definition

```csharp
public sealed record FamilyCreatedEvent(
    FamilyId FamilyId,
    FamilyName FamilyName,
    UserId OwnerId,
    DateTime CreatedAt
) : DomainEvent;
```

## Publishing (in aggregate)

```csharp
family.RaiseDomainEvent(new FamilyCreatedEvent(...));
```

Use `RaiseDomainEvent()` on aggregates. Cleared with `ClearDomainEvents()`.

## Handler (Wolverine)

Static class, auto-discovered. No `INotificationHandler<T>` interface.

```csharp
public static class InvitationSentEventHandler
{
    public static async Task Handle(
        InvitationSentEvent @event,
        IEmailService emailService,
        IFamilyRepository familyRepository,
        CancellationToken ct)
    {
        // Handle event
    }
}
```

## Rules

- Events are sealed records extending `DomainEvent`
- Location: `Features/{Module}/Domain/Events/{Name}Event.cs`
- Handlers: `Features/{Module}/Application/EventHandlers/{Name}Handler.cs`
- Handlers are static classes with `Handle()` method (Wolverine)
- Use past tense: Created, Sent, Accepted, Declined, Revoked
