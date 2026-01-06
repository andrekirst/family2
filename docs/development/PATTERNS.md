# DDD & Architecture Patterns

**Purpose:** Domain-Driven Design patterns, value objects, aggregates, domain events, and architectural patterns used throughout Family Hub.

**When to reference:** Implementing new features, understanding module boundaries, working with domain events.

---

## DDD Module Boundaries

**Architecture:** Modular Monolith (Phase 1-4) → Microservices (Phase 5+)

**8 Modules:** Auth, Calendar, Task, Shopping, Health, Meal Planning, Finance, Communication

Each module owns:

- Domain entities and aggregates
- Domain events (RabbitMQ)
- GraphQL schema types
- PostgreSQL schema (Row-Level Security)

**Full specification:** [domain-model-microservices-map.md](../architecture/domain-model-microservices-map.md)

---

## Value Object Patterns

### When to Use Value Objects

Use Vogen value objects for:

- **Identifiers:** UserId, FamilyId, TaskId, etc.
- **Email addresses:** Email with validation
- **Names:** FamilyName, UserName with max length
- **Descriptions:** TaskDescription, EventDescription
- **Amounts:** Money, Quantity with validation
- **Dates/Times:** When you need domain validation beyond DateTime

**DON'T use for:**

- Simple primitive types without domain logic
- DTOs or data transfer objects
- Database query results

### Value Object Design Principles

1. **Immutability:** Value objects are immutable (readonly struct)
2. **Self-validation:** Validation happens in Validate() method
3. **Equality by value:** Two value objects with same value are equal
4. **No identity:** Value objects have no unique ID

### Common Patterns

#### String-based Value Objects with Validation

```csharp
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyName
{
    private const int MaxLength = 50;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Family name cannot be empty");

        if (value.Length > MaxLength)
            return Validation.Invalid($"Family name cannot exceed {MaxLength} characters");

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
        => input?.Trim() ?? string.Empty;
}
```

#### GUID-based Identifiers

```csharp
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct UserId
{
    public static UserId New() => From(Guid.NewGuid());
}
```

#### Money/Amount Value Objects

```csharp
[ValueObject<decimal>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Money
{
    private static Validation Validate(decimal value)
    {
        if (value < 0)
            return Validation.Invalid("Amount cannot be negative");

        if (value > 1_000_000)
            return Validation.Invalid("Amount exceeds maximum allowed");

        return Validation.Ok;
    }
}
```

### Value Object Location

- **Shared value objects:** `/src/api/FamilyHub.SharedKernel/Domain/ValueObjects/`
- **Module-specific:** `/src/api/Modules/FamilyHub.Modules.{Module}/Domain/ValueObjects/`

---

## Aggregate Patterns

### What is an Aggregate?

An aggregate is a cluster of domain objects that can be treated as a single unit. One entity is the aggregate root, which ensures consistency.

### Aggregate Rules

1. **One aggregate root** - External objects reference only the root
2. **Consistency boundary** - All changes go through the root
3. **Transaction boundary** - Save/load aggregates as a unit
4. **Small aggregates** - Keep aggregates small for performance

### Example: Family Aggregate

```csharp
public class Family : AggregateRoot<FamilyId>
{
    public FamilyName Name { get; private set; }
    public UserId OwnerId { get; private set; }
    private List<FamilyMember> _members = new();
    public IReadOnlyList<FamilyMember> Members => _members.AsReadOnly();

    // Constructor (factory method pattern)
    public static Family Create(FamilyName name, UserId ownerId)
    {
        var family = new Family
        {
            Id = FamilyId.New(),
            Name = name,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };

        // Raise domain event
        family.RaiseDomainEvent(new FamilyCreatedDomainEvent(family.Id, name, ownerId));

        return family;
    }

    // Business logic methods
    public void AddMember(UserId userId, FamilyRole role)
    {
        // Validate business rules
        if (_members.Any(m => m.UserId == userId))
            throw new DomainException("User is already a member");

        var member = new FamilyMember(userId, role);
        _members.Add(member);

        // Raise domain event
        RaiseDomainEvent(new FamilyMemberAddedDomainEvent(Id, userId, role));
    }

    public void Rename(FamilyName newName)
    {
        if (Name == newName) return;

        var oldName = Name;
        Name = newName;

        RaiseDomainEvent(new FamilyRenamedDomainEvent(Id, oldName, newName));
    }

    // Private constructor for EF Core
    private Family() { }
}
```

### Aggregate Design Guidelines

- **Keep small:** 1 aggregate root + 0-5 child entities
- **Model behavior:** Methods represent use cases
- **Protect invariants:** All changes through methods, not property setters
- **Raise events:** Domain events for cross-aggregate communication

---

## Domain Event Patterns

### When to Use Domain Events

Use domain events for:

- **Cross-module communication** - Calendar notifies Task when appointment created
- **Event chains** - Doctor appointment triggers calendar, task, shopping events
- **Asynchronous processing** - Email notifications, audit logs
- **Eventual consistency** - Data synchronization across aggregates

### Domain Event Structure

```csharp
public record FamilyCreatedDomainEvent(
    FamilyId FamilyId,
    FamilyName Name,
    UserId OwnerId
) : IDomainEvent;
```

### Raising Domain Events

```csharp
public abstract class AggregateRoot<TId> where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

### Publishing Domain Events

```csharp
// In SaveChangesAsync
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var domainEvents = ChangeTracker.Entries<AggregateRoot>()
        .SelectMany(e => e.Entity.DomainEvents)
        .ToList();

    var result = await base.SaveChangesAsync(cancellationToken);

    foreach (var domainEvent in domainEvents)
    {
        await _mediator.Publish(domainEvent, cancellationToken);
    }

    return result;
}
```

### Handling Domain Events

```csharp
public class FamilyCreatedDomainEventHandler
    : INotificationHandler<FamilyCreatedDomainEvent>
{
    private readonly IMessageBus _messageBus;

    public FamilyCreatedDomainEventHandler(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task Handle(
        FamilyCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        // Publish integration event to RabbitMQ
        await _messageBus.PublishAsync(
            new FamilyCreatedIntegrationEvent(
                notification.FamilyId.Value,
                notification.Name.Value,
                notification.OwnerId.Value
            ),
            cancellationToken
        );
    }
}
```

---

## Event Chain Patterns

**Primary differentiator:** Automatic cross-domain workflows that no competitor offers.

### Example: Doctor Appointment Event Chain

```
Doctor Appointment Created (Health Module)
  ↓
Calendar Event Created (Calendar Module)
  ↓
Preparation Task Created (Task Module)
  ↓
Prescription Added (Health Module)
  ↓
Medication Shopping Item (Shopping Module)
  ↓
Pickup Task Created (Task Module)
  ↓
Refill Reminder Scheduled (Health Module)
```

### Event Chain Implementation

**Pattern:** Saga orchestration with MediatR and RabbitMQ

```csharp
public class DoctorAppointmentEventChainOrchestrator
    : INotificationHandler<HealthAppointmentScheduledEvent>
{
    private readonly IMediator _mediator;
    private readonly IMessageBus _messageBus;

    public async Task Handle(
        HealthAppointmentScheduledEvent notification,
        CancellationToken cancellationToken)
    {
        // Step 1: Create calendar event
        var calendarCommand = new CreateCalendarEventCommand(
            notification.AppointmentId,
            notification.DateTime,
            notification.Duration,
            $"Doctor: {notification.DoctorName}"
        );
        await _mediator.Send(calendarCommand, cancellationToken);

        // Step 2: Create preparation task (24 hours before)
        var taskCommand = new CreateTaskCommand(
            notification.FamilyId,
            "Prepare for doctor appointment",
            notification.DateTime.AddDays(-1)
        );
        await _mediator.Send(taskCommand, cancellationToken);

        // Publish integration event for other modules
        await _messageBus.PublishAsync(
            new AppointmentChainInitiatedEvent(notification),
            cancellationToken
        );
    }
}
```

**Full specifications:** [event-chains-reference.md](../architecture/event-chains-reference.md)

---

## Repository Patterns

### Interface (in Domain layer)

```csharp
public interface IFamilyRepository
{
    Task<Family?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Family>> GetByOwnerIdAsync(UserId ownerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(FamilyName name, CancellationToken cancellationToken = default);
    Task AddAsync(Family family, CancellationToken cancellationToken = default);
    Task UpdateAsync(Family family, CancellationToken cancellationToken = default);
    Task DeleteAsync(Family family, CancellationToken cancellationToken = default);
}
```

### Implementation (in Infrastructure layer)

```csharp
public class FamilyRepository : IFamilyRepository
{
    private readonly AuthDbContext _context;

    public FamilyRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Family?> GetByIdAsync(
        FamilyId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        Family family,
        CancellationToken cancellationToken = default)
    {
        await _context.Families.AddAsync(family, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // Other methods...
}
```

### Repository Guidelines

- **One repository per aggregate root**
- **Return domain models, not EF entities**
- **Use async methods**
- **Include navigation properties when needed**
- **Let DbContext handle change tracking**

---

## Command/Query Pattern (CQRS Light)

### Command (Write Operation)

```csharp
public record CreateFamilyCommand(FamilyName Name) : IRequest<CreateFamilyPayload>;

public class CreateFamilyCommandHandler
    : IRequestHandler<CreateFamilyCommand, CreateFamilyPayload>
{
    private readonly IFamilyRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public CreateFamilyCommandHandler(
        IFamilyRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<CreateFamilyPayload> Handle(
        CreateFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // Business logic
        var userId = _currentUser.UserId;
        var family = Family.Create(request.Name, userId);

        await _repository.AddAsync(family, cancellationToken);

        return new CreateFamilyPayload(family);
    }
}
```

### Query (Read Operation)

```csharp
public record GetUserFamiliesQuery(UserId UserId) : IRequest<GetUserFamiliesResult>;

public class GetUserFamiliesQueryHandler
    : IRequestHandler<GetUserFamiliesQuery, GetUserFamiliesResult>
{
    private readonly IFamilyRepository _repository;

    public GetUserFamiliesQueryHandler(IFamilyRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetUserFamiliesResult> Handle(
        GetUserFamiliesQuery request,
        CancellationToken cancellationToken)
    {
        var families = await _repository.GetByOwnerIdAsync(
            request.UserId,
            cancellationToken
        );

        return new GetUserFamiliesResult(families);
    }
}
```

### CQRS Guidelines

- **Commands:** Validate, execute, return result
- **Queries:** Read-only, no side effects
- **Handlers:** One handler per command/query
- **Validation:** Use FluentValidation for complex rules

---

**Last updated:** 2026-01-06
**Version:** 1.0.0
