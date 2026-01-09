# Coding Standards

**Purpose:** Authoritative coding standards for Family Hub based on architectural decisions, established patterns, and team conventions.

**Scope:** C# backend (.NET 10), TypeScript frontend (Angular v21), testing practices, DDD patterns, GraphQL APIs.

**Status:** Living document - updated as patterns evolve.

**Last Updated:** 2026-01-07

---

## Table of Contents

1. [Quick Reference](#quick-reference)
2. [C# & .NET Standards](#c--net-standards)
3. [DDD & Architecture Patterns](#ddd--architecture-patterns)
4. [Event-Driven Architecture](#event-driven-architecture)
5. [GraphQL & API Design](#graphql--api-design)
6. [Testing Standards](#testing-standards)
7. [TypeScript & Angular Standards](#typescript--angular-standards)
8. [Performance & Scalability](#performance--scalability)
9. [Security & Privacy](#security--privacy)
10. [Developer Experience](#developer-experience)
11. [Anti-Patterns to Avoid](#anti-patterns-to-avoid)

---

## Quick Reference

### Essential Patterns at a Glance

```csharp
// ✅ Primary constructors with DI - Trust the container
public sealed class FamilyRepository(AuthDbContext context) : IFamilyRepository
{
    // No null checks for DI parameters - container guarantees non-null
}

// ✅ Method parameter validation - Explicit checks
public async Task AddAsync(Family family, CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(family);
    await _context.Families.AddAsync(family, cancellationToken);
}

// ✅ Logger source generation - ALWAYS use for logging
public sealed partial class CreateFamilyCommandHandler(ILogger<CreateFamilyCommandHandler> logger)
{
    [LoggerMessage(LogLevel.Information, "Creating family '{familyName}' for user {userId}")]
    partial void LogCreatingFamily(FamilyName familyName, Guid userId);
}

// ✅ Vogen value objects - Format validation
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value) { /* validation */ }
    private static string NormalizeInput(string input) => input.Trim().ToLowerInvariant();
}

// ✅ Behavior-driven mutation - private setters
public class Family : AggregateRoot<FamilyId>
{
    public FamilyName Name { get; private set; }

    public void Rename(FamilyName newName)
    {
        if (Name == newName) return;
        Name = newName;
        RaiseDomainEvent(new FamilyRenamedEvent(Id, Name, newName));
    }
}

// ✅ Result pattern - Typed errors (not exceptions for business logic)
public record CreateFamilyResult
{
    public FamilyId? FamilyId { get; init; }
    public FamilyName? Name { get; init; }
    public Error? Error { get; init; }
    public bool IsSuccess => Error == null;
}
```

### Testing Quick Reference

```csharp
// ✅ FluentAssertions - ALWAYS use (never xUnit Assert)
result.Should().NotBeNull();
family.Name.Should().Be(expectedName);
await act.Should().ThrowAsync<DomainException>();

// ✅ AutoNSubstitute - Use for infrastructure tests
[Theory, AutoNSubstituteData]
public async Task CreateFamily_Success(
    IFamilyRepository repository,
    CreateFamilyCommand command)
{
    // Arrange
    repository.ExistsByNameAsync(Arg.Any<FamilyName>()).Returns(false);

    // Act & Assert
}

// ✅ Manual test data - Use for domain logic tests
var email = Email.From("test@example.com");
var userId = UserId.From(Guid.NewGuid());
```

### TypeScript Quick Reference

```typescript
// ✅ Angular Signals - State management
export class FamilyService {
  currentFamily = signal<Family | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);
}

// ✅ Smart/Dumb component pattern
// Smart (Container) - Logic & state
@Component({...})
export class FamilyWizardPageComponent {
  familyService = inject(FamilyService);
  async onWizardComplete() { /* logic */ }
}

// Dumb (Presentational) - Pure UI
@Component({...})
export class ButtonComponent {
  @Input() variant: 'primary' | 'secondary' = 'primary';
  @Output() clicked = new EventEmitter<Event>();
}

// ✅ Typed reactive forms
interface FamilyFormData {
  name: string;
  preferences: FamilyPreferences;
}
```

---

## C# & .NET Standards

### 1. Dependency Injection Pattern

**Decision:** Trust DI container - no redundant null checks for injected dependencies.

**Rationale:** Modern DI containers (.NET built-in) guarantee non-null dependencies. Defensive null checks add noise without value.

#### ✅ Correct Pattern

```csharp
// Primary constructor - direct parameter usage
public sealed class FamilyRepository(AuthDbContext context) : IFamilyRepository
{
    // DO NOT null-check DI parameters - trust the container

    public async Task<Family?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken)
    {
        return await context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }
}

// Multiple dependencies
public sealed class CreateFamilyCommandHandler(
    IUserContext userContext,
    IFamilyRepository familyRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateFamilyCommandHandler> logger)
    : IRequestHandler<CreateFamilyCommand, CreateFamilyResult>
{
    // All dependencies available as constructor parameters
    // No need for redundant field declarations with null checks
}
```

#### ❌ Anti-Pattern

```csharp
// DON'T: Redundant null checks for DI parameters
public sealed class FamilyRepository(AuthDbContext context) : IFamilyRepository
{
    private readonly AuthDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    // ❌ Unnecessary - DI container guarantees non-null
}
```

**Exception:** Keep explicit field initialization with null checks ONLY when:

- Working with legacy code that predates primary constructors
- Interfacing with third-party libraries with unknown behavior
- Documenting intentional defensive programming (rare)

**Current Status:** Transitioning from explicit null checks to trust-based pattern. New code MUST use trust-based pattern.

---

### 2. Null Checking Strategy

**Multi-layer approach:** Different strategies for different scenarios.

#### DI Parameters (Constructor Injection)

```csharp
// ✅ Trust DI container - no null checks
public sealed class UserService(IUserRepository repository, ILogger<UserService> logger)
{
    // Direct parameter usage - no validation needed
}
```

#### Method Parameters

```csharp
// ✅ Explicit null checks using ArgumentNullException.ThrowIfNull
public async Task AddAsync(Family family, CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(family);
    await _context.Families.AddAsync(family, cancellationToken);
}

// ✅ Use for public API methods
public async Task UpdateAsync(Family family, CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(family);
    // Implementation
}
```

#### Nullable Reference Types

```csharp
// ✅ Enable in all projects
<Nullable>enable</Nullable>

// ✅ Use nullable operators
public Family? GetFamily() => _cache.TryGetValue(key, out var family) ? family : null;

// ✅ Null-forgiving operator when you KNOW value is non-null
var user = _context.User!; // Only when 100% certain
```

---

### 3. Immutability Strategy

**Decision:** Behavior-driven mutation with private setters (NOT init-only properties).

**Rationale:** Domain entities need controlled mutation for business logic. Init-only restricts legitimate business operations.

#### ✅ Correct Pattern: Private Setters + Behavior Methods

```csharp
public class Family : AggregateRoot<FamilyId>
{
    public FamilyName Name { get; private set; }
    public UserId OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private List<User> _members = new();
    public IReadOnlyList<User> Members => _members.AsReadOnly();

    // Factory method for creation
    public static Family Create(FamilyName name, UserId ownerId)
    {
        var family = new Family
        {
            Id = FamilyId.New(),
            Name = name,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };

        family.RaiseDomainEvent(new FamilyCreatedEvent(family.Id, name, ownerId));
        return family;
    }

    // Behavior method for mutations
    public void Rename(FamilyName newName)
    {
        if (Name == newName) return;

        var oldName = Name;
        Name = newName;

        RaiseDomainEvent(new FamilyRenamedEvent(Id, oldName, newName));
    }

    public void AddMember(User user)
    {
        if (_members.Any(m => m.Id == user.Id))
            throw new DomainException("User is already a member");

        _members.Add(user);
        RaiseDomainEvent(new MemberAddedEvent(Id, user.Id));
    }

    // Private constructor for EF Core
    private Family() { }
}
```

#### ❌ Anti-Pattern: Init-only Properties

```csharp
// DON'T: Init-only prevents legitimate mutations
public class Family
{
    public FamilyName Name { get; init; } // ❌ Can't rename family!
    public UserId OwnerId { get; init; }  // ❌ Can't transfer ownership!
}
```

**Guidelines:**

- **Aggregates/Entities:** `private set` + behavior methods
- **Value Objects:** Immutable by design (Vogen generates readonly struct)
- **DTOs/Records:** `init` is fine (no business logic)
- **Collections:** Expose as `IReadOnlyList<T>`, mutate via methods

---

### 4. Logging Standards

**Decision:** Source generation for ALL logging - no manual ILogger.Log calls.

**Rationale:**

- Compile-time validation (no runtime string formatting errors)
- Better performance (no boxing, no string interpolation overhead)
- Structured logging by default
- Easier to maintain

#### ✅ Correct Pattern: LoggerMessage Source Generation

```csharp
public sealed partial class CreateFamilyCommandHandler(
    ILogger<CreateFamilyCommandHandler> logger)
{
    public async Task<CreateFamilyResult> Handle(CreateFamilyCommand request)
    {
        LogCreatingFamily(request.Name, userId.Value);

        // Business logic

        LogFamilyCreatedSuccessfully(family.Id.Value, family.Name, userId.Value);

        return result;
    }

    // ✅ Source-generated logging methods
    [LoggerMessage(LogLevel.Information, "Creating family '{familyName}' for user {userId}")]
    partial void LogCreatingFamily(FamilyName familyName, Guid userId);

    [LoggerMessage(LogLevel.Information, "Successfully created family {familyId} '{familyName}' with owner {userId}")]
    partial void LogFamilyCreatedSuccessfully(Guid familyId, FamilyName familyName, Guid userId);

    [LoggerMessage(LogLevel.Warning, "Family creation failed: {reason}")]
    partial void LogFamilyCreationFailed(string reason);

    [LoggerMessage(LogLevel.Error, "Error creating family: {errorMessage}")]
    partial void LogErrorCreatingFamily(string errorMessage, Exception ex);
}
```

#### ❌ Anti-Pattern: Manual ILogger Calls

```csharp
// DON'T: Manual logging
_logger.LogInformation($"Creating family {request.Name} for user {userId}"); // ❌
_logger.LogInformation("Creating family {familyName} for user {userId}", request.Name, userId); // ❌
```

**Naming Convention:**

- Methods: `Log{Action}{Context}` (e.g., `LogCreatingFamily`, `LogAuthorizationFailed`)
- Be descriptive but concise
- Include relevant context (IDs, names, states)

**Performance Impact:**

- Source generation: ~10-20% faster than manual logging
- Zero allocations for common scenarios
- Compile-time safety (no typos in format strings)

---

## DDD & Architecture Patterns

### 5. Aggregate Design

**Decision:** Small aggregates with eventual consistency.

**Rationale:** Large aggregates cause:

- Concurrency conflicts (transaction scope too wide)
- Performance issues (loading unnecessary data)
- Coupling across bounded contexts

#### ✅ Correct Pattern: Small, Focused Aggregates

```csharp
// ✅ Family aggregate - focused on family core data
public class Family : AggregateRoot<FamilyId>
{
    public FamilyName Name { get; private set; }
    public UserId OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Only direct members - no nested aggregates
    private List<User> _members = new();
    public IReadOnlyList<User> Members => _members.AsReadOnly();

    // Cross-aggregate references by ID only
    public List<InvitationId> PendingInvitationIds { get; private set; } = new();

    // Business logic focused on family operations
    public void Rename(FamilyName newName) { /* ... */ }
    public void AddMember(User user) { /* ... */ }
    public void RemoveMember(UserId userId) { /* ... */ }
}

// ✅ Separate aggregate for invitations
public class FamilyMemberInvitation : AggregateRoot<InvitationId>
{
    public FamilyId FamilyId { get; private set; } // Reference by ID
    public Email InviteeEmail { get; private set; }
    public InvitationStatus Status { get; private set; }
    public UserRole Role { get; private set; }

    // Invitation-specific logic
    public void Accept(UserId userId) { /* ... */ }
    public void Reject() { /* ... */ }
    public void Cancel() { /* ... */ }
}
```

#### ❌ Anti-Pattern: Large Aggregates

```csharp
// DON'T: Family aggregate containing all related data
public class Family : AggregateRoot<FamilyId>
{
    public List<FamilyMemberInvitation> Invitations { get; set; } // ❌
    public List<CalendarEvent> Events { get; set; }                // ❌
    public List<Task> Tasks { get; set; }                          // ❌
    public ShoppingList ShoppingList { get; set; }                 // ❌
    // This creates a "god aggregate" that's impossible to manage
}
```

**Aggregate Sizing Guidelines:**

- **Root + 0-5 child entities** maximum
- **Single transaction boundary** (one save operation)
- **Cross-aggregate references by ID** (not object references)
- **Use eventual consistency** for cross-aggregate operations

**Coordination Pattern:**

```csharp
// ✅ Coordinate via domain events (eventual consistency)
public class Family
{
    public void AddMember(User user)
    {
        _members.Add(user);
        RaiseDomainEvent(new MemberAddedEvent(Id, user.Id));
        // Other aggregates react to this event asynchronously
    }
}

// Event handler updates related aggregates
public class MemberAddedEventHandler : INotificationHandler<MemberAddedEvent>
{
    public async Task Handle(MemberAddedEvent notification)
    {
        // Update Calendar aggregate to share events with new member
        // Update Task aggregate to show family tasks to new member
        // All via separate transactions (eventual consistency)
    }
}
```

---

### 6. Validation Strategy

**Decision:** Multi-layer validation with clear separation of concerns.

**Layers:**

1. **Vogen (Format Validation):** String format, length, regex patterns
2. **Domain (Invariants):** Business rules within aggregate
3. **Application (Business Rules):** Cross-aggregate rules, authorization

#### Layer 1: Vogen Format Validation

```csharp
// ✅ Vogen: Format validation only
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email cannot be empty.");

        if (value.Length > 320) // RFC 5321
            return Validation.Invalid("Email cannot exceed 320 characters.");

        if (!EmailRegex.IsMatch(value))
            return Validation.Invalid("Email format is invalid.");

        return Validation.Ok;
    }

    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();
}
```

#### Layer 2: Domain Invariants

```csharp
// ✅ Domain: Aggregate invariants
public class Family : AggregateRoot<FamilyId>
{
    private const int MaxMembers = 50;

    public void AddMember(User user)
    {
        // Domain rule: Single-aggregate invariant
        if (_members.Count >= MaxMembers)
            throw new DomainException($"Family cannot exceed {MaxMembers} members");

        if (_members.Any(m => m.Id == user.Id))
            throw new DomainException("User is already a member");

        _members.Add(user);
        RaiseDomainEvent(new MemberAddedEvent(Id, user.Id));
    }
}
```

#### Layer 3: Application Business Rules

```csharp
// ✅ Application: Cross-aggregate rules, FluentValidation
public class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    private readonly IFamilyRepository _familyRepository;

    public CreateFamilyCommandValidator(IFamilyRepository familyRepository)
    {
        _familyRepository = familyRepository;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Family name is required");

        RuleFor(x => x.Name)
            .MustAsync(async (name, ct) =>
            {
                // Cross-aggregate validation: Check uniqueness
                return !await _familyRepository.ExistsByNameAsync(name, ct);
            })
            .WithMessage("A family with this name already exists");
    }
}

// Application authorization (MediatR pipeline behavior)
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
    {
        // Application-level authorization checks
        if (request is IRequireOwnerRole && !_userContext.IsOwner())
            throw new UnauthorizedAccessException("Owner role required");

        return await next();
    }
}
```

**Validation Decision Tree:**

```
Is it a format/syntax rule (email regex, max length)?
  YES → Vogen Validate()

Is it an invariant of a single aggregate?
  YES → Domain entity method

Does it involve multiple aggregates or external systems?
  YES → FluentValidation in Application layer

Is it an authorization check?
  YES → MediatR pipeline behavior
```

---

### 6.1. Validation Cache Key Management

**Pattern:** Centralized cache key construction for `IValidationCache`.

**Problem:** Validators cache entities to eliminate duplicate database queries in handlers. String literal cache keys scattered across validators and handlers create risk of typos, copy-paste errors, and cache mismatches.

**Solution:** Use `CacheKeyBuilder` static helper for type-safe, consistent cache keys.

#### ✅ Correct Pattern: CacheKeyBuilder

```csharp
// In Validator: Store entities in cache
public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator(
        IFamilyMemberInvitationRepository invitationRepository,
        IFamilyRepository familyRepository,
        IValidationCache validationCache)
    {
        RuleFor(x => x.Token)
            .CustomAsync(async (token, context, cancellationToken) =>
            {
                var invitation = await invitationRepository.GetByTokenAsync(token, cancellationToken);
                var family = await familyRepository.GetByIdAsync(invitation.FamilyId, cancellationToken);

                // ✅ Use CacheKeyBuilder for consistent keys
                validationCache.Set(CacheKeyBuilder.FamilyMemberInvitation(token.Value), invitation);
                validationCache.Set(CacheKeyBuilder.Family(invitation.FamilyId.Value), family);
            });
    }
}

// In Handler: Retrieve entities from cache
public class AcceptInvitationCommandHandler
{
    public async Task<Result> Handle(AcceptInvitationCommand request)
    {
        // ✅ Use same CacheKeyBuilder methods - guaranteed match
        var invitation = validationCache.Get<FamilyMemberInvitationAggregate>(
            CacheKeyBuilder.FamilyMemberInvitation(request.Token.Value));

        var family = validationCache.Get<FamilyAggregate>(
            CacheKeyBuilder.Family(invitation.FamilyId.Value));
    }
}
```

#### ❌ Anti-Pattern: String Literal Cache Keys

```csharp
// DON'T: String literals with typo risk
validationCache.Set($"FamilyMemberInvitationAggregate:{token}", invitation); // ❌ Typo in prefix
var invitation = validationCache.Get<T>($"FamilyMemberInvitation:{token}");  // ❌ Mismatch!

// DON'T: Manual string interpolation
validationCache.Set($"Family:{familyId.Value}", family);  // ❌ Duplication
validationCache.Get<Family>($"Family:{familyId.Value}"); // ❌ No type safety
```

**Benefits:**

1. **Compile-Time Safety:** Method names verified by compiler, prevents typos
2. **Single Source of Truth:** One place to define/change key format per entity
3. **Refactoring Safe:** Change entity name in one place, everywhere updates
4. **Self-Documenting:** Method signature shows required parameter types
5. **Guard Clauses:** Builder validates inputs (null/empty checks)

**When to Add New Entity Types:**

```csharp
// In CacheKeyBuilder.cs:
public static class CacheKeyBuilder
{
    // Add new method when caching a new entity type
    public static string User(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        return $"User:{userId}";
    }
}
```

**Pattern Origin:** Prevents cache key mismatches like the AcceptInvitation bug where validator used `FamilyMemberInvitation:{token}` but handler looked for `FamilyMemberInvitationAggregate:{token}`, causing cache miss.

---

### 7. Domain Events vs Integration Events

**Pattern:** Domain events (in-memory) → Integration events (RabbitMQ).

**Domain Events:** Internal to the bounded context, raised by aggregates.

```csharp
// ✅ Domain event: Internal, raised by aggregate
public record FamilyCreatedEvent(
    FamilyId FamilyId,
    FamilyName Name,
    UserId OwnerId
) : IDomainEvent;

public class Family
{
    public static Family Create(FamilyName name, UserId ownerId)
    {
        var family = new Family { /* ... */ };
        family.RaiseDomainEvent(new FamilyCreatedEvent(family.Id, name, ownerId));
        return family;
    }
}

// Domain event published in-memory via MediatR
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
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

**Integration Events:** Cross-bounded-context communication via RabbitMQ.

```csharp
// ✅ Integration event: Cross-module, published to RabbitMQ
public record FamilyCreatedIntegrationEvent(
    Guid FamilyId,
    string Name,
    Guid OwnerId,
    DateTime Timestamp,
    Guid CorrelationId
) : IIntegrationEvent;

// Domain event handler publishes integration event
public class FamilyCreatedEventHandler : INotificationHandler<FamilyCreatedEvent>
{
    private readonly IMessageBus _messageBus;

    public async Task Handle(FamilyCreatedEvent notification, CancellationToken ct)
    {
        // Transform domain event → integration event
        var integrationEvent = new FamilyCreatedIntegrationEvent(
            notification.FamilyId.Value,
            notification.Name.Value,
            notification.OwnerId.Value,
            DateTime.UtcNow,
            Guid.NewGuid()
        );

        await _messageBus.PublishAsync(integrationEvent, ct);
    }
}
```

**Guidelines:**

- **Domain events:** Aggregate state changes, same transaction
- **Integration events:** Cross-module communication, async, durable
- **Never skip domain events:** Always raise domain events even if no integration event needed
- **Transformation layer:** Domain event handler converts to integration event

---

## Event-Driven Architecture

### 8. Event Chain Pattern (Saga Choreography)

**Decision:** Saga choreography (event-driven reactions) over orchestration.

**Rationale:** Better decoupling, no central coordinator, natural fit for event chains.

#### ✅ Correct Pattern: Choreography (Event-Driven Reactions)

```csharp
// ✅ Each service reacts to events independently

// 1. Health Service: User schedules doctor appointment
public class HealthService
{
    public async Task ScheduleAppointment(ScheduleAppointmentCommand cmd)
    {
        var appointment = Appointment.Schedule(/* ... */);
        await _repository.AddAsync(appointment);

        // Publishes: HealthAppointmentScheduledEvent
    }
}

// 2. Calendar Service: Reacts to appointment event
public class AppointmentScheduledEventHandler
    : IIntegrationEventHandler<HealthAppointmentScheduledEvent>
{
    public async Task Handle(HealthAppointmentScheduledEvent evt)
    {
        // Create calendar event for appointment
        var calendarEvent = CalendarEvent.Create(/* ... */);
        await _repository.AddAsync(calendarEvent);

        // Publishes: CalendarEventCreatedEvent
    }
}

// 3. Task Service: Reacts to appointment event
public class AppointmentTaskCreationHandler
    : IIntegrationEventHandler<HealthAppointmentScheduledEvent>
{
    public async Task Handle(HealthAppointmentScheduledEvent evt)
    {
        // Create preparation task (24h before)
        var task = Task.Create(
            "Prepare questions for Dr. " + evt.DoctorName,
            evt.AppointmentTime.AddHours(-24)
        );
        await _repository.AddAsync(task);

        // Publishes: TaskCreatedEvent
    }
}

// 4. Communication Service: Reacts to appointment event
public class AppointmentNotificationHandler
    : IIntegrationEventHandler<HealthAppointmentScheduledEvent>
{
    public async Task Handle(HealthAppointmentScheduledEvent evt)
    {
        // Schedule 3 notifications: 24h before, 2h before, after
        await _notificationScheduler.Schedule(/* ... */);
    }
}
```

#### ❌ Anti-Pattern: Orchestration (Central Coordinator)

```csharp
// DON'T: Central saga orchestrator
public class AppointmentSagaOrchestrator
{
    public async Task Execute(ScheduleAppointmentCommand cmd)
    {
        // ❌ Tight coupling: Orchestrator knows about all services
        var appointment = await _healthService.ScheduleAppointment(cmd);
        var calendarEvent = await _calendarService.CreateEvent(appointment);
        var task = await _taskService.CreateTask(appointment);
        await _communicationService.ScheduleNotifications(appointment);

        // ❌ Single point of failure
        // ❌ Hard to evolve (adding new steps requires orchestrator changes)
    }
}
```

**Choreography Benefits:**

- **Decoupling:** Services don't know about each other
- **Extensibility:** Add new reactions without changing existing code
- **Resilience:** One service failure doesn't block others
- **Scalability:** Independent scaling per service

**See:** [event-chains-reference.md](../architecture/event-chains-reference.md) for full event chain specifications.

---

### 9. Event Versioning

**Decision:** Explicit version suffixes with parallel handlers.

**Rationale:** Gradual migration, backward compatibility, clear intent.

#### ✅ Correct Pattern: Versioned Events

```csharp
// V1: Original event
public record FamilyCreatedEventV1(
    Guid FamilyId,
    string Name,
    Guid OwnerId
) : IIntegrationEvent;

// V2: Added creation timestamp and preferences
public record FamilyCreatedEventV2(
    Guid FamilyId,
    string Name,
    Guid OwnerId,
    DateTime CreatedAt,
    FamilyPreferences Preferences
) : IIntegrationEvent;

// Both handlers run in parallel during migration
public class FamilyCreatedV1Handler : IIntegrationEventHandler<FamilyCreatedEventV1>
{
    public async Task Handle(FamilyCreatedEventV1 evt)
    {
        // Handle V1 events from old services
    }
}

public class FamilyCreatedV2Handler : IIntegrationEventHandler<FamilyCreatedEventV2>
{
    public async Task Handle(FamilyCreatedEventV2 evt)
    {
        // Handle V2 events from new services
    }
}

// Publisher migration
public class FamilyService
{
    public async Task CreateFamily(CreateFamilyCommand cmd)
    {
        var family = Family.Create(/* ... */);
        await _repository.AddAsync(family);

        // Publish V2 event (new version)
        await _messageBus.PublishAsync(new FamilyCreatedEventV2(
            family.Id.Value,
            family.Name.Value,
            family.OwnerId.Value,
            family.CreatedAt,
            family.Preferences
        ));
    }
}
```

**Migration Steps:**

1. Create V2 event with new fields
2. Deploy V2 handler alongside V1 handler (both running)
3. Migrate publishers to V2 event (gradual rollout)
4. Monitor: Confirm no V1 events in last 7 days
5. Remove V1 handler

**Naming Convention:**

- `{EventName}V1`, `{EventName}V2`, etc.
- No version suffix = V1 (for first version)
- Always add version when creating V2+ (don't rename V1)

---

### 10. Idempotency & Compensation

**Idempotency Decision:** Event ID tracking in database.

**Compensation Decision:** Automatic retry with exponential backoff.

#### Idempotency Pattern

```csharp
// ✅ Event handler with idempotency check
public class FamilyCreatedEventHandler : IIntegrationEventHandler<FamilyCreatedEvent>
{
    private readonly IEventIdempotencyStore _idempotencyStore;

    public async Task Handle(FamilyCreatedEvent evt, CancellationToken ct)
    {
        // Check if event already processed
        if (await _idempotencyStore.HasProcessedAsync(evt.EventId, ct))
        {
            _logger.LogInformation("Event {eventId} already processed, skipping", evt.EventId);
            return;
        }

        // Process event
        var calendar = Calendar.CreateForFamily(evt.FamilyId);
        await _repository.AddAsync(calendar, ct);

        // Mark as processed (in same transaction)
        await _idempotencyStore.MarkProcessedAsync(evt.EventId, ct);
    }
}

// Database table for tracking
public class ProcessedEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; }
    public DateTime ProcessedAt { get; set; }
}
```

#### Compensation Pattern (Automatic Retry)

```csharp
// ✅ Retry with exponential backoff
public class RabbitMQConsumer
{
    public async Task ConsumeAsync<TEvent>(TEvent evt, CancellationToken ct)
    {
        const int maxRetries = 5;
        int retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                await _handler.Handle(evt, ct);
                return; // Success
            }
            catch (Exception ex)
            {
                retryCount++;

                if (retryCount >= maxRetries)
                {
                    // Move to dead-letter queue for manual investigation
                    await _deadLetterQueue.PublishAsync(evt, ex);
                    _logger.LogError("Event {eventId} failed after {retries} retries", evt.EventId, maxRetries);
                    throw;
                }

                // Exponential backoff: 1s, 2s, 4s, 8s, 16s
                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount - 1));
                _logger.LogWarning("Event processing failed, retrying in {delay}s (attempt {attempt}/{max})",
                    delay.TotalSeconds, retryCount, maxRetries);

                await Task.Delay(delay, ct);
            }
        }
    }
}
```

**Dead Letter Queue Handling:**

```csharp
// Manual compensation for failed events
public class DeadLetterQueueProcessor
{
    public async Task ProcessFailedEvent(ProcessedEvent evt)
    {
        // 1. Investigate root cause
        // 2. Fix data/code issue
        // 3. Replay event manually
        // 4. Or trigger compensating action

        switch (evt.EventType)
        {
            case "FamilyCreated":
                // Compensating action: Delete family if calendar creation failed
                await _familyService.DeleteAsync(evt.FamilyId);
                break;

            case "AppointmentScheduled":
                // Retry with manual data correction
                await ReplayEvent(evt);
                break;
        }
    }
}
```

**Guidelines:**

- **Idempotency:** ALWAYS check event ID before processing
- **Retry:** 5 attempts with exponential backoff (1s, 2s, 4s, 8s, 16s)
- **Dead Letter Queue:** Manual review for permanently failed events
- **Compensation:** Prefer automatic retry over manual compensation

---

## GraphQL & API Design

### 11. Input/Command Pattern

**Decision:** Separate GraphQL Input DTOs (primitives) → MediatR Commands (Vogen).

**Rationale:** HotChocolate can't deserialize Vogen value objects from JSON. Separate DTOs provide explicit conversion point.

**Future Migration:** When HotChocolate supports Vogen, migrate to command-as-input pattern.

#### ✅ Current Pattern (Phase 0-4)

```csharp
// GraphQL Input DTO - primitives only
public record CreateFamilyInput
{
    public string Name { get; init; } = string.Empty;
}

// MediatR Command - Vogen value objects
public record CreateFamilyCommand(FamilyName Name)
    : IRequest<CreateFamilyResult>;

// GraphQL mutation - explicit mapping
[Mutation]
public async Task<CreateFamilyPayload> CreateFamilyAsync(
    CreateFamilyInput input,
    [Service] IMediator mediator,
    CancellationToken ct)
{
    // Explicit conversion: Input → Command
    var command = new CreateFamilyCommand(
        FamilyName.From(input.Name) // Validation happens here
    );

    var result = await mediator.Send(command, ct);

    return new CreateFamilyPayload
    {
        FamilyId = result.FamilyId,
        Name = result.Name,
        Errors = result.Errors
    };
}
```

#### 🔮 Future Pattern (When HotChocolate Supports Vogen)

```csharp
// Command used directly as input
[Mutation]
public async Task<CreateFamilyPayload> CreateFamilyAsync(
    CreateFamilyCommand command, // ✅ Direct deserialization
    [Service] IMediator mediator,
    CancellationToken ct)
{
    return await mediator.Send(command, ct);
}
```

**See:** [ADR-003: GraphQL Input/Command Pattern](../architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)

---

### 12. Error Handling Pattern

**Decision:** Result pattern with typed errors (no exceptions for business logic).

#### ✅ Result Pattern

```csharp
// ✅ Result type with typed errors
public record CreateFamilyResult
{
    public FamilyId? FamilyId { get; init; }
    public FamilyName? Name { get; init; }
    public UserId? OwnerId { get; init; }
    public List<Error>? Errors { get; init; }

    public bool IsSuccess => Errors == null || Errors.Count == 0;
}

public record Error(string Code, string Message);

// ✅ GraphQL payload with errors
public class CreateFamilyPayload
{
    public FamilyId? FamilyId { get; set; }
    public FamilyName? Name { get; set; }
    public List<Error>? Errors { get; set; }
}

// ✅ Handler returns result (not throwing exceptions)
public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, CreateFamilyResult>
{
    public async Task<CreateFamilyResult> Handle(CreateFamilyCommand request, CancellationToken ct)
    {
        // Check business rule
        if (await _repository.ExistsByNameAsync(request.Name, ct))
        {
            return new CreateFamilyResult
            {
                Errors = new List<Error>
                {
                    new Error("FAMILY_NAME_EXISTS", "A family with this name already exists")
                }
            };
        }

        var family = Family.Create(request.Name, _userContext.UserId);
        await _repository.AddAsync(family, ct);

        return new CreateFamilyResult
        {
            FamilyId = family.Id,
            Name = family.Name,
            OwnerId = family.OwnerId
        };
    }
}
```

**GraphQL Client Handling:**

```typescript
// ✅ TypeScript client checks errors
const result = await this.graphql.mutate(CREATE_FAMILY_MUTATION, { input });

if (result.createFamily.errors && result.createFamily.errors.length > 0) {
  this.error.set(result.createFamily.errors[0].message);
  return;
}

this.currentFamily.set(result.createFamily.family);
```

**Exception vs Result:**

```csharp
// ❌ Exceptions for business logic
if (await _repository.ExistsByNameAsync(request.Name))
    throw new FamilyNameExistsException(); // DON'T

// ✅ Result for business logic failures
return new CreateFamilyResult
{
    Errors = new List<Error> { new Error("FAMILY_NAME_EXISTS", "...") }
};

// ✅ Exceptions for technical failures
if (await _database.IsDown())
    throw new DatabaseConnectionException(); // OK - infrastructure failure
```

---

### 13. GraphQL Schema Organization

**Decision:** Module-based type extensions with namespacing.

#### ✅ Schema Organization

```csharp
// ✅ Module-specific type extensions
// /Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/Types/AuthTypeExtensions.cs
[ExtendObjectType("Query")]
public class AuthQueryExtensions
{
    [GraphQLName("auth_userFamilies")]
    public async Task<GetUserFamiliesResult> GetUserFamilies(
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetUserFamiliesQuery();
        return await mediator.Send(query, ct);
    }
}

[ExtendObjectType("Mutation")]
public class AuthMutationExtensions
{
    [GraphQLName("auth_createFamily")]
    public async Task<CreateFamilyPayload> CreateFamily(
        CreateFamilyInput input,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var command = new CreateFamilyCommand(FamilyName.From(input.Name));
        return await mediator.Send(command, ct);
    }
}

// ✅ Registration
builder.Services
    .AddGraphQLServer()
    .AddTypeExtensionsFromAssembly(typeof(AuthTypeExtensions).Assembly);
```

**Namespacing Convention:**

- `{module}_{operation}`: `auth_createFamily`, `calendar_createEvent`
- Prevents name collisions across modules
- Clear module ownership in schema

---

### 14. Authorization Pattern

**Decision:** Marker interfaces with MediatR pipeline behavior.

#### ✅ Authorization Markers

```csharp
// ✅ Marker interfaces for authorization
public interface IPublicQuery { }
public interface IRequireAuthentication { }
public interface IRequireFamilyContext { }
public interface IRequireOwnerRole { }
public interface IRequireAdminRole { }
public interface IRequireOwnerOrAdminRole { }

// ✅ Apply to commands/queries
public record GetUserFamiliesQuery()
    : IRequest<GetUserFamiliesResult>,
      IRequireAuthentication; // ✅ Requires auth

public record GetPendingInvitationsQuery()
    : IRequest<GetPendingInvitationsResult>,
      IRequireFamilyContext,      // ✅ Requires family membership
      IRequireOwnerOrAdminRole;   // ✅ Requires Owner or Admin role

public record GetAuthUrlQuery()
    : IRequest<GetAuthUrlResult>,
      IPublicQuery; // ✅ Public endpoint
```

#### ✅ MediatR Pipeline Behavior

```csharp
public class AuthorizationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Skip authorization for public queries
        if (request is IPublicQuery)
            return await next(ct);

        // Check family context
        if (request is IRequireFamilyContext && !_userContext.HasFamily())
            throw new UnauthorizedAccessException("User does not belong to a family");

        // Role-based authorization
        var policyName = request switch
        {
            IRequireOwnerRole => AuthorizationPolicyConstants.RequireOwner,
            IRequireAdminRole => AuthorizationPolicyConstants.RequireAdmin,
            IRequireOwnerOrAdminRole => AuthorizationPolicyConstants.RequireOwnerOrAdmin,
            _ => null
        };

        if (policyName != null)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                _userContext.Principal,
                policyName);

            if (!authResult.Succeeded)
                throw new UnauthorizedAccessException($"Policy '{policyName}' failed");
        }

        return await next(ct);
    }
}
```

**See:** Example implementation in `/src/api/Modules/FamilyHub.Modules.Auth/Application/Behaviors/AuthorizationBehavior.cs`

---

## Testing Standards

### 15. Test Pyramid Strategy

**Decision:** E2E heavy for event chains: 50% unit, 20% integration, 30% E2E.

**Rationale:** Event chains are core differentiator - must be tested end-to-end.

#### Test Distribution

```
        /\
       /  \
      / E2E \ 30%    ← API-first event chain tests
     /--------\
    /          \
   / Integration\ 20% ← Real RabbitMQ via TestContainers
  /--------------\
 /                \
/   Unit Tests     \ 50% ← Domain logic, value objects, handlers
--------------------
```

#### Unit Tests (50%)

```csharp
// ✅ Domain logic tests - FluentAssertions + AutoFixture
[Theory, AutoNSubstituteData]
public async Task CreateFamily_Success_ReturnsFamily(
    IFamilyRepository repository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
{
    // Arrange - Manual test data for domain
    var familyName = FamilyName.From("Smith Family");
    var userId = UserId.From(Guid.NewGuid());
    var user = new User(userId, Email.From("test@example.com"));

    userContext.UserId.Returns(userId);
    userContext.User.Returns(user);

    repository.ExistsByNameAsync(Arg.Any<FamilyName>(), Arg.Any<CancellationToken>())
        .Returns(false);

    var command = new CreateFamilyCommand(familyName);
    var handler = new CreateFamilyCommandHandler(userContext, repository, unitOfWork, _logger);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert - FluentAssertions
    result.Should().NotBeNull();
    result.FamilyId.Should().NotBeNull();
    result.Name.Should().Be(familyName);

    await repository.Received(1).AddAsync(
        Arg.Is<Family>(f => f.Name == familyName),
        Arg.Any<CancellationToken>()
    );
}
```

#### Integration Tests (20%)

```csharp
// ✅ Integration tests - Real database + RabbitMQ via TestContainers
public class CreateFamilyIntegrationTests : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task CreateFamily_WithRealDatabase_Success()
    {
        // Arrange - Real database via TestContainers
        await using var factory = new GraphQlTestFactory(_containerFixture);
        var client = factory.CreateClient();

        // Act - GraphQL mutation
        var response = await client.PostAsync("/graphql", new StringContent(
            JsonSerializer.Serialize(new
            {
                query = @"
                    mutation {
                      auth_createFamily(input: { name: ""Smith Family"" }) {
                        familyId
                        name
                        errors { message }
                      }
                    }"
            })
        ));

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse>();
        result.Data.auth_createFamily.errors.Should().BeNullOrEmpty();
        result.Data.auth_createFamily.familyId.Should().NotBeEmpty();
    }
}
```

#### E2E Tests (30%)

```typescript
// ✅ API-first E2E tests for event chains (Playwright)
test('Doctor appointment event chain triggers all downstream events', async ({
  page,
  rabbitmq, // Custom fixture with RabbitMQ connection
}) => {
  // STEP 1: Create appointment via GraphQL API (not UI)
  const mutation = `
    mutation {
      scheduleHealthAppointment(input: {
        doctorName: "Dr. Smith"
        appointmentTime: "${futureDate}"
      }) {
        appointment { id }
      }
    }
  `;

  const result = await graphqlClient.mutate(mutation);
  const appointmentId = result.scheduleHealthAppointment.appointment.id;

  // STEP 2: Verify HealthAppointmentScheduledEvent published to RabbitMQ
  const appointmentEvent = await rabbitmq.waitForMessage(
    (msg) => msg.eventType === 'HealthAppointmentScheduled',
    5000
  );
  expect(appointmentEvent).not.toBeNull();

  // STEP 3: Verify CalendarEventCreatedEvent published
  const calendarEvent = await rabbitmq.waitForMessage(
    (msg) => msg.eventType === 'CalendarEventCreated',
    5000
  );
  expect(calendarEvent.data.title).toBe('Doctor: Dr. Smith');

  // STEP 4: Verify TaskCreatedEvent published
  const taskEvent = await rabbitmq.waitForMessage(
    (msg) => msg.eventType === 'TaskCreated' && msg.data.title.includes('Prepare questions'),
    5000
  );
  expect(taskEvent).not.toBeNull();

  // STEP 5: Query backend to verify entities created
  const calendarQuery = await graphqlClient.query(`
    query { calendarEvents { id title } }
  `);
  expect(calendarQuery.calendarEvents).toContainEqual(
    expect.objectContaining({ title: 'Doctor: Dr. Smith' })
  );

  // STEP 6: Optional UI spot-check
  await page.goto('/calendar');
  await expect(page.getByText('Doctor: Dr. Smith')).toBeVisible();
});
```

**See:** `/src/frontend/family-hub-web/e2e/tests/event-chains.spec.ts` for full template.

---

### 16. Event Chain Testing Strategy

**Decision:** Real RabbitMQ via TestContainers for all tests (unit/integration/E2E).

#### ✅ RabbitMQ TestContainers Setup

```typescript
// ✅ Playwright fixture with RabbitMQ
import { test as base } from '@playwright/test';
import { RabbitMQContainer } from '@testcontainers/rabbitmq';

export const test = base.extend<{ rabbitmq: RabbitMQHelper }>({
  rabbitmq: async ({}, use) => {
    const container = await new RabbitMQContainer('rabbitmq:3.12-management')
      .withExposedPorts(5672, 15672)
      .start();

    const helper = new RabbitMQHelper(container.getConnectionString());
    await helper.connect();

    await use(helper);

    await helper.disconnect();
    await container.stop();
  },
});

export class RabbitMQHelper {
  async waitForMessage(predicate: (msg: any) => boolean, timeoutMs: number = 5000): Promise<any> {
    const startTime = Date.now();

    while (Date.now() - startTime < timeoutMs) {
      const messages = await this.consumeMessages();
      const found = messages.find(predicate);
      if (found) return found;

      await new Promise((resolve) => setTimeout(resolve, 100));
    }

    return null;
  }
}
```

**Benefits:**

- **Real behavior:** Tests actual event flow, not mocks
- **Fast:** TestContainers starts in ~3 seconds
- **Reliable:** Consistent across environments
- **CI-friendly:** Works in GitHub Actions

---

### 17. Test Data Strategy

**Decision:** AutoFixture for infrastructure, manual for domain.

#### ✅ Infrastructure Tests: AutoFixture

```csharp
// ✅ AutoFixture for repositories, services, controllers
[Theory, AutoNSubstituteData]
public async Task GetByIdAsync_Success(
    [Frozen] AuthDbContext context, // AutoFixture injects
    FamilyRepository repository,
    FamilyId familyId)
{
    // AutoFixture generates test data
    var family = context.Families.Add(new Family { Id = familyId });
    await context.SaveChangesAsync();

    var result = await repository.GetByIdAsync(familyId);

    result.Should().NotBeNull();
    result!.Id.Should().Be(familyId);
}
```

#### ✅ Domain Tests: Manual Creation

```csharp
// ✅ Manual creation for domain logic (clarity over convenience)
[Fact]
public void Family_Create_RaisesFamilyCreatedEvent()
{
    // Arrange - Explicit, readable test data
    var familyName = FamilyName.From("Smith Family");
    var ownerId = UserId.From(Guid.NewGuid());

    // Act
    var family = Family.Create(familyName, ownerId);

    // Assert
    family.DomainEvents.Should().ContainSingle(e => e is FamilyCreatedEvent);
    var evt = family.DomainEvents.OfType<FamilyCreatedEvent>().Single();
    evt.Name.Should().Be(familyName);
    evt.OwnerId.Should().Be(ownerId);
}
```

**Rationale:**

- **Domain tests:** Clarity matters - explicit test data shows intent
- **Infrastructure tests:** Speed matters - AutoFixture reduces boilerplate
- **Vogen objects:** ALWAYS create manually (better error messages)

---

### 18. E2E Testing Philosophy

**Decision:** API-first for event chains, UI for user journeys.

**Zero-retry policy:** Tests must pass on first run.

#### ✅ API-First Event Chain Tests

```typescript
// ✅ Test event chain via API (10x faster than UI)
test('Prescription workflow creates shopping list and task', async ({ rabbitmq }) => {
  // 1. Create prescription via GraphQL (not UI)
  const result = await graphqlClient.mutate(CREATE_PRESCRIPTION_MUTATION, {
    medicationName: 'Amoxicillin',
    dosage: '500mg',
  });

  // 2. Verify events published
  await rabbitmq.waitForMessage((msg) => msg.eventType === 'PrescriptionIssued');
  await rabbitmq.waitForMessage((msg) => msg.eventType === 'ShoppingItemAdded');
  await rabbitmq.waitForMessage((msg) => msg.eventType === 'TaskCreated');

  // 3. Query backend to verify entities
  const shopping = await graphqlClient.query(GET_SHOPPING_LISTS_QUERY);
  expect(shopping.shoppingLists[0].items).toContainEqual(
    expect.objectContaining({ name: 'Amoxicillin 500mg' })
  );
});
```

#### ✅ UI Tests for User Journeys

```typescript
// ✅ UI test for critical user flow
test('User can create family and invite members', async ({ page }) => {
  // 1. Navigate to family creation wizard
  await page.goto('/family/create');

  // 2. Fill family name
  await page.getByLabel('Family Name').fill('Smith Family');
  await page.getByRole('button', { name: 'Create Family' }).click();

  // 3. Verify redirect to dashboard
  await expect(page).toHaveURL('/dashboard');
  await expect(page.getByText('Smith Family')).toBeVisible();

  // 4. Invite member
  await page.getByRole('button', { name: 'Invite Member' }).click();
  await page.getByLabel('Email').fill('john@example.com');
  await page.getByRole('button', { name: 'Send Invitation' }).click();

  // 5. Verify invitation sent
  await expect(page.getByText('Invitation sent to john@example.com')).toBeVisible();
});
```

**Guidelines:**

- **Event chains:** API-first testing (GraphQL + RabbitMQ)
- **User journeys:** UI testing (Playwright)
- **Zero retries:** Tests MUST be deterministic
- **Wait strategies:** Use `waitForMessage()` for events, `expect().toBeVisible()` for UI

**See:** [ADR-004: Playwright Migration](../architecture/ADR-004-PLAYWRIGHT-MIGRATION.md)

---

## TypeScript & Angular Standards

### 19. Component Architecture

**Decision:** Smart/dumb (container/presentational) pattern.

#### ✅ Smart Component (Container)

```typescript
// ✅ Smart component: Business logic, state management, API calls
@Component({
  selector: 'app-family-wizard-page',
  standalone: true,
  imports: [CommonModule, WizardComponent],
  template: `
    <app-wizard [steps]="wizardSteps" (complete)="onWizardComplete($event)"></app-wizard>
  `,
})
export class FamilyWizardPageComponent implements OnInit {
  // ✅ Services injected
  familyService = inject(FamilyService);
  private router = inject(Router);

  // ✅ Configuration
  wizardSteps: WizardStepConfig[] = [
    /* ... */
  ];

  // ✅ Business logic
  async onWizardComplete(event: Map<string, unknown>): Promise<void> {
    const familyNameData = event.get('family-name') as FamilyNameStepData;

    if (!familyNameData?.name) {
      console.error('Missing family name');
      return;
    }

    await this.familyService.createFamily(familyNameData.name.trim());

    if (this.familyService.error()) {
      console.error('Family creation failed');
      return;
    }

    this.router.navigate(['/dashboard']);
  }
}
```

#### ✅ Dumb Component (Presentational)

```typescript
// ✅ Dumb component: Pure UI, inputs/outputs only
@Component({
  selector: 'app-button',
  standalone: true,
  template: `
    <button
      [type]="type"
      [disabled]="disabled || loading"
      [class]="buttonClasses"
      (click)="handleClick($event)"
    >
      @if (loading) {
      <span class="animate-spin mr-2">⟳</span>
      }
      <ng-content></ng-content>
    </button>
  `,
})
export class ButtonComponent {
  // ✅ Inputs for configuration
  @Input() variant: 'primary' | 'secondary' | 'tertiary' = 'primary';
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() disabled = false;
  @Input() loading = false;

  // ✅ Outputs for events
  @Output() clicked = new EventEmitter<Event>();

  // ✅ Pure computed properties (no side effects)
  get buttonClasses(): string {
    return `${this.baseClasses} ${this.variantClasses[this.variant]} ${
      this.sizeClasses[this.size]
    }`;
  }

  handleClick(event: Event): void {
    if (!this.disabled && !this.loading) {
      this.clicked.emit(event);
    }
  }
}
```

**Guidelines:**

- **Smart components:** 1 per page/feature, orchestrate logic
- **Dumb components:** Reusable UI, no services, pure functions
- **No services in dumb components** (except utility services like i18n)
- **Inputs/outputs only** for dumb components

---

### 20. State Management

**Decision:** Angular Signals exclusively (no NgRx, no RxJS state).

#### ✅ Signal-Based State

```typescript
// ✅ Service with signals
@Injectable({ providedIn: 'root' })
export class FamilyService {
  private graphql = inject(GraphQLService);

  // ✅ Signals for reactive state
  currentFamily = signal<Family | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);

  // ✅ Computed signals
  hasFamily = computed(() => this.currentFamily() !== null);
  familyName = computed(() => this.currentFamily()?.name ?? 'Unknown');

  // ✅ State mutation methods
  async loadCurrentFamily(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);

    try {
      const result = await this.graphql.query(GET_USER_FAMILIES_QUERY);

      if (result.errors) {
        this.error.set(result.errors[0].message);
        return;
      }

      this.currentFamily.set(result.data.families[0] ?? null);
    } catch (err) {
      this.error.set('Failed to load family');
    } finally {
      this.isLoading.set(false);
    }
  }

  async createFamily(name: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);

    try {
      const result = await this.graphql.mutate(CREATE_FAMILY_MUTATION, { name });

      if (result.createFamily.errors) {
        this.error.set(result.createFamily.errors[0].message);
        return;
      }

      this.currentFamily.set(result.createFamily.family);
    } catch (err) {
      this.error.set('Failed to create family');
    } finally {
      this.isLoading.set(false);
    }
  }
}
```

#### ✅ Component Using Signals

```typescript
@Component({
  template: `
    @if (familyService.isLoading()) {
    <app-spinner />
    } @else if (familyService.error()) {
    <div class="error">{{ familyService.error() }}</div>
    } @else if (familyService.hasFamily()) {
    <h1>{{ familyService.familyName() }}</h1>
    }
  `,
})
export class DashboardComponent {
  familyService = inject(FamilyService);

  ngOnInit() {
    // Effect for side effects
    effect(() => {
      if (this.familyService.hasFamily()) {
        console.log('Family loaded:', this.familyService.currentFamily());
      }
    });
  }
}
```

**Guidelines:**

- **All state in signals** (no BehaviorSubject, no manual change detection)
- **Computed for derived state** (no manual recalculation)
- **Effect for side effects** (logging, analytics, etc.)
- **Async pipe NOT needed** (signals trigger change detection automatically)

---

### 21. Form Handling

**Decision:** Typed reactive forms with schema validation.

#### ✅ Typed Reactive Forms

```typescript
// ✅ Form interface
interface FamilyFormData {
  name: string;
  preferences: {
    theme: 'light' | 'dark';
    notifications: boolean;
  };
}

@Component({...})
export class FamilySettingsComponent {
  private fb = inject(FormBuilder);

  // ✅ Typed form group
  familyForm = this.fb.group<FamilyFormData>({
    name: this.fb.nonNullable.control('', [
      Validators.required,
      Validators.maxLength(50)
    ]),
    preferences: this.fb.group({
      theme: this.fb.nonNullable.control<'light' | 'dark'>('light'),
      notifications: this.fb.nonNullable.control(true)
    })
  });

  // ✅ Typed form submission
  onSubmit(): void {
    if (this.familyForm.invalid) {
      this.familyForm.markAllAsTouched();
      return;
    }

    const formData: FamilyFormData = this.familyForm.getRawValue();
    this.familyService.updateFamily(formData);
  }

  // ✅ Field-level validation messages
  getErrorMessage(field: string): string {
    const control = this.familyForm.get(field);

    if (control?.hasError('required')) return 'This field is required';
    if (control?.hasError('maxlength')) return 'Maximum 50 characters';

    return '';
  }
}
```

**Template:**

```html
<form [formGroup]="familyForm" (ngSubmit)="onSubmit()">
  <app-input label="Family Name" formControlName="name" [error]="getErrorMessage('name')" />

  <app-select
    label="Theme"
    formControlName="theme"
    [options]="[
      { value: 'light', label: 'Light' },
      { value: 'dark', label: 'Dark' }
    ]"
  />

  <app-button type="submit" [disabled]="familyForm.invalid"> Save Changes </app-button>
</form>
```

---

### 22. Accessibility Standards

**Decision:** Multi-layer approach - design + review + automated tests.

#### Design Phase

```typescript
// ✅ Semantic HTML
<nav aria-label="Main navigation">
  <ul role="list">
    <li><a href="/dashboard">Dashboard</a></li>
    <li><a href="/calendar">Calendar</a></li>
  </ul>
</nav>

// ✅ ARIA labels for screen readers
<button aria-label="Close modal" (click)="close()">
  <span aria-hidden="true">&times;</span>
</button>

// ✅ Keyboard navigation
<div
  role="button"
  tabindex="0"
  (click)="handleClick()"
  (keydown.enter)="handleClick()"
  (keydown.space)="handleClick()"
>
  Custom Button
</div>
```

#### Automated Testing

```typescript
// ✅ Playwright accessibility tests
test('Family creation wizard is accessible', async ({ page }) => {
  await page.goto('/family/create');

  // Run axe accessibility scan
  const accessibilityScanResults = await new AxeBuilder({ page }).analyze();

  expect(accessibilityScanResults.violations).toEqual([]);
});

// ✅ Keyboard navigation test
test('Can navigate wizard with keyboard only', async ({ page }) => {
  await page.goto('/family/create');

  await page.keyboard.press('Tab'); // Focus first input
  await page.keyboard.type('Smith Family');
  await page.keyboard.press('Tab'); // Focus submit button
  await page.keyboard.press('Enter'); // Submit form

  await expect(page).toHaveURL('/dashboard');
});
```

**WCAG AA Compliance Checklist:**

- [ ] All images have alt text
- [ ] Color contrast ratio ≥ 4.5:1 for normal text
- [ ] All interactive elements keyboard accessible
- [ ] Focus indicators visible (outline/ring)
- [ ] Form inputs have associated labels
- [ ] Error messages announced to screen readers
- [ ] Page titles descriptive and unique

---

## Performance & Scalability

### 23. EF Core Tracking Strategy

**Decision:** No-tracking for queries, tracking for commands.

#### ✅ Queries: AsNoTracking()

```csharp
// ✅ Query handlers: AsNoTracking for read-only
public class GetUserFamiliesQueryHandler : IRequestHandler<GetUserFamiliesQuery, GetUserFamiliesResult>
{
    public async Task<GetUserFamiliesResult> Handle(GetUserFamiliesQuery request, CancellationToken ct)
    {
        var families = await _context.Families
            .AsNoTracking() // ✅ No tracking overhead
            .Include(f => f.Members)
            .Where(f => f.Members.Any(m => m.Id == request.UserId))
            .ToListAsync(ct);

        return new GetUserFamiliesResult { Families = families };
    }
}
```

#### ✅ Commands: Tracking Enabled

```csharp
// ✅ Command handlers: Tracking enabled for updates
public class UpdateFamilyCommandHandler : IRequestHandler<UpdateFamilyCommand, UpdateFamilyResult>
{
    public async Task<UpdateFamilyResult> Handle(UpdateFamilyCommand request, CancellationToken ct)
    {
        // ✅ Tracking enabled by default (no AsNoTracking)
        var family = await _context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, ct);

        if (family == null)
            return new UpdateFamilyResult { Error = "Family not found" };

        // ✅ EF Core tracks changes automatically
        family.Rename(request.NewName);

        await _context.SaveChangesAsync(ct);

        return new UpdateFamilyResult { Family = family };
    }
}
```

**Performance Impact:**

- **AsNoTracking:** ~30% faster queries, 50% less memory
- **Tracking:** Necessary for updates, automatic change detection

---

### 24. Caching Strategy

**Decision:** Repository-level caching for stable aggregates.

#### ✅ In-Memory Cache for Reference Data

```csharp
// ✅ Cache stable data (roles, categories, settings)
public class UserRoleRepository : IUserRoleRepository
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

    public async Task<List<UserRole>> GetAllRolesAsync(CancellationToken ct)
    {
        const string cacheKey = "user_roles_all";

        if (_cache.TryGetValue(cacheKey, out List<UserRole> cachedRoles))
            return cachedRoles;

        var roles = await _context.UserRoles
            .AsNoTracking()
            .ToListAsync(ct);

        _cache.Set(cacheKey, roles, _cacheExpiration);

        return roles;
    }
}
```

#### ✅ Cache Invalidation

```csharp
// ✅ Invalidate cache on updates
public class UpdateUserRoleCommandHandler
{
    public async Task<UpdateUserRoleResult> Handle(UpdateUserRoleCommand request, CancellationToken ct)
    {
        var role = await _repository.GetByIdAsync(request.RoleId, ct);
        role.UpdatePermissions(request.Permissions);

        await _unitOfWork.SaveChangesAsync(ct);

        // ✅ Invalidate cache
        _cache.Remove("user_roles_all");

        return new UpdateUserRoleResult { Role = role };
    }
}
```

**What to Cache:**

- ✅ Reference data (roles, categories, settings)
- ✅ Expensive computed values (analytics, reports)
- ❌ User-specific data (families, tasks) - too dynamic

---

### 25. Pagination Strategy

**Decision:** Cursor-based pagination with Relay spec.

#### ✅ Cursor-Based Pagination (GraphQL)

```csharp
// ✅ GraphQL query with cursor pagination
[Query]
[UsePaging] // HotChocolate Relay pagination
[UseFiltering]
[UseSorting]
public IQueryable<Family> GetFamilies([Service] AuthDbContext context)
{
    return context.Families
        .AsNoTracking()
        .OrderBy(f => f.CreatedAt);
}

// Client query
/*
query {
  families(first: 10, after: "cursor123") {
    edges {
      cursor
      node {
        id
        name
        createdAt
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
      startCursor
      endCursor
    }
  }
}
*/
```

**Benefits:**

- **Stable:** New items don't affect cursor position
- **Efficient:** Index-optimized (no OFFSET scan)
- **Relay compatible:** Works with Apollo Client, Relay

---

### 26. Message Durability Strategy

**Decision:** Persistent messages for critical events, transient for notifications.

#### ✅ Persistent Messages (Critical Events)

```csharp
// ✅ Critical events: Durable, persistent, acknowledged
public class RabbitMQPublisher
{
    public async Task PublishAsync<TEvent>(TEvent evt, CancellationToken ct)
        where TEvent : IIntegrationEvent
    {
        var properties = _channel.CreateBasicProperties();

        // ✅ Critical events: Persistent
        if (evt is FamilyCreatedEvent ||
            evt is AppointmentScheduledEvent ||
            evt is PrescriptionIssuedEvent)
        {
            properties.Persistent = true; // ✅ Survives broker restart
            properties.DeliveryMode = 2;  // ✅ Persistent delivery
        }
        else
        {
            properties.Persistent = false; // ✅ Transient (notifications)
            properties.DeliveryMode = 1;
        }

        var json = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: "family_hub_events",
            routingKey: evt.GetType().Name,
            basicProperties: properties,
            body: body
        );
    }
}
```

**Guidelines:**

- **Persistent:** Domain events, state changes, financial data
- **Transient:** Notifications, logs, metrics
- **Trade-off:** Persistent = slower but reliable

---

## Security & Privacy

### 27. Authorization Strategy

**Decision:** Application-level authorization only (no PostgreSQL RLS).

**Rationale:** Simpler to maintain, better performance, easier debugging.

#### ✅ Application-Level Authorization

```csharp
// ✅ MediatR pipeline behavior for all requests
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // Check family context
        if (request is IRequireFamilyContext)
        {
            if (_userContext.FamilyId == FamilyId.Empty)
                throw new UnauthorizedAccessException("User must belong to a family");
        }

        // Check role-based permissions
        if (request is IRequireOwnerRole)
        {
            if (_userContext.Role != UserRole.Owner)
                throw new UnauthorizedAccessException("Owner role required");
        }

        return await next(ct);
    }
}

// ✅ Repository enforces family context
public class FamilyRepository
{
    public async Task<Family?> GetByIdAsync(FamilyId id, CancellationToken ct)
    {
        // ✅ Filter by user's family (application-level)
        return await _context.Families
            .Where(f => f.Id == id && f.Id == _userContext.FamilyId)
            .FirstOrDefaultAsync(ct);
    }
}
```

**Why NOT PostgreSQL RLS:**

- Application-level authorization sufficient for family context (one family per user)
- RLS adds complexity (policies, role switching, debugging overhead)
- Performance: Application filtering faster than RLS policies
- Future: May add RLS in Phase 5+ for compliance requirements

---

### 28. Encryption Strategy

**Decision:** Encryption at rest (database/disk level) only.

**Rationale:** Database encryption sufficient for SaaS. Application-level encryption unnecessary complexity.

#### ✅ Database-Level Encryption

```yaml
# PostgreSQL configuration
# Encryption at rest via cloud provider (AWS RDS, Azure Database)
ssl: true
ssl_min_protocol_version: TLSv1.2

# Connection string
Host=postgres.example.com;
Database=familyhub;
SSL Mode=Require;
Trust Server Certificate=false;
```

**What IS encrypted:**

- ✅ Database at rest (cloud provider encryption)
- ✅ Database connections (TLS 1.2+)
- ✅ API traffic (HTTPS only)
- ✅ RabbitMQ connections (TLS)

**What is NOT encrypted:**

- ❌ Application-level field encryption (unnecessary for family data)
- ❌ Searchable encryption (not needed)

---

### 29. Token Management

**Decision:** Refresh token rotation with revocation list.

#### ✅ Refresh Token Rotation

```csharp
// ✅ Token refresh with rotation
public class TokenService
{
    public async Task<TokenResult> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        // 1. Validate refresh token
        var tokenData = await _tokenRepository.GetByTokenAsync(refreshToken, ct);

        if (tokenData == null || tokenData.ExpiresAt < DateTime.UtcNow)
        {
            // ✅ Invalid/expired token - add to revocation list
            await _revokedTokenRepository.AddAsync(refreshToken, ct);
            throw new InvalidRefreshTokenException();
        }

        // 2. Revoke old refresh token (rotation)
        await _revokedTokenRepository.AddAsync(refreshToken, ct);
        await _tokenRepository.DeleteAsync(tokenData, ct);

        // 3. Generate new access token + refresh token
        var newAccessToken = GenerateAccessToken(tokenData.UserId);
        var newRefreshToken = GenerateRefreshToken();

        // 4. Store new refresh token
        await _tokenRepository.AddAsync(new RefreshToken
        {
            Token = newRefreshToken,
            UserId = tokenData.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        }, ct);

        return new TokenResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600
        };
    }
}
```

**Benefits:**

- **Rotation:** Old refresh tokens invalidated after use
- **Revocation list:** Prevents token reuse
- **Short-lived access tokens:** 1 hour expiration
- **Long-lived refresh tokens:** 7 days

---

### 30. Audit Logging Strategy

**Decision:** Domain events as audit trail.

#### ✅ Domain Events = Audit Log

```csharp
// ✅ All state changes raise domain events
public class Family
{
    public void Rename(FamilyName newName)
    {
        var oldName = Name;
        Name = newName;

        // ✅ Audit trail: Domain event captures change
        RaiseDomainEvent(new FamilyRenamedEvent(Id, oldName, newName, DateTime.UtcNow));
    }

    public void AddMember(User user)
    {
        _members.Add(user);

        // ✅ Audit trail: Who was added, when, by whom
        RaiseDomainEvent(new MemberAddedEvent(
            Id,
            user.Id,
            _userContext.UserId,
            DateTime.UtcNow
        ));
    }
}

// ✅ Event handler stores audit log
public class AuditLogEventHandler : INotificationHandler<IDomainEvent>
{
    public async Task Handle(IDomainEvent evt, CancellationToken ct)
    {
        var auditEntry = new AuditLog
        {
            EventType = evt.GetType().Name,
            EventData = JsonSerializer.Serialize(evt),
            Timestamp = DateTime.UtcNow,
            UserId = _userContext.UserId,
            FamilyId = _userContext.FamilyId
        };

        await _auditRepository.AddAsync(auditEntry, ct);
    }
}
```

**Benefits:**

- **Automatic:** All domain events logged
- **Complete:** No missed audit entries
- **Queryable:** JSON data for analysis
- **GDPR compliant:** Can delete user's audit trail

---

## Developer Experience

### 31. Documentation Philosophy

**Decision:** Self-documenting code with strategic comments.

#### ✅ Self-Documenting Code

```csharp
// ✅ Clear naming - no comment needed
public async Task<Family?> GetFamilyByUserIdAsync(UserId userId, CancellationToken ct)
{
    return await _context.Families
        .Include(f => f.Members)
        .FirstOrDefaultAsync(f => f.Members.Any(m => m.Id == userId), ct);
}

// ❌ Bad naming - comment required
public async Task<Family?> GetAsync(Guid id, CancellationToken ct) // ❌ What ID? User ID? Family ID?
```

#### ✅ Strategic Comments (When Needed)

```csharp
// ✅ XML documentation for public APIs
/// <summary>
/// Creates a new family and establishes owner membership.
/// User context is automatically provided by UserContextEnrichmentBehavior.
/// </summary>
/// <param name="request">Command containing family name.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Result containing created family or errors.</returns>
public async Task<CreateFamilyResult> Handle(CreateFamilyCommand request, CancellationToken ct)

// ✅ Explaining non-obvious business logic
public void AddMember(User user)
{
    // Business rule: Limit enforced to prevent database performance issues
    // with large family queries. See ADR-005 for rationale.
    if (_members.Count >= MaxMembers)
        throw new DomainException($"Family cannot exceed {MaxMembers} members");
}

// ✅ Explaining workarounds/hacks
// WORKAROUND: HotChocolate can't deserialize Vogen types from JSON.
// Using separate Input DTO until issue resolved. See ADR-003.
var command = new CreateFamilyCommand(FamilyName.From(input.Name));
```

#### ❌ Avoid Obvious Comments

```csharp
// ❌ Comment states the obvious
// Get family by ID
var family = await _repository.GetByIdAsync(id);

// ❌ Comment duplicates method name
// Rename the family
public void Rename(FamilyName newName) { }
```

**Guidelines:**

- **Prefer:** Expressive naming, small methods, type safety
- **Add comments for:** Non-obvious business rules, workarounds, ADR references
- **XML docs for:** Public APIs, libraries, complex algorithms

---

### 32. Debugging Strategy

**Decision:** Correlation IDs with structured logging.

#### ✅ Correlation ID Propagation

```csharp
// ✅ MediatR pipeline behavior adds correlation ID
public class CorrelationIdBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestType"] = typeof(TRequest).Name
        }))
        {
            _logger.LogInformation("Starting request {requestType} with correlation {correlationId}",
                typeof(TRequest).Name, correlationId);

            var result = await next(ct);

            _logger.LogInformation("Completed request {requestType} with correlation {correlationId}",
                typeof(TRequest).Name, correlationId);

            return result;
        }
    }
}

// ✅ Integration events include correlation ID
public record FamilyCreatedEvent(
    Guid EventId,
    Guid CorrelationId, // ✅ Trace across services
    FamilyId FamilyId,
    FamilyName Name,
    UserId OwnerId
) : IIntegrationEvent;
```

#### ✅ Structured Logging Query

```bash
# Query logs by correlation ID
kubectl logs -l app=familyhub-api | grep "correlationId=abc123"

# Trace event chain across services
SELECT * FROM logs
WHERE correlation_id = 'abc123'
ORDER BY timestamp;
```

**Benefits:**

- **End-to-end tracing:** Follow request across services
- **Event chain debugging:** See entire workflow
- **Performance analysis:** Identify bottlenecks

---

### 33. AI Onboarding Strategy

**Decision:** Layered documentation - quick reference + deep dive.

**Documentation Layers:**

1. **Quick Start:** [CLAUDE.md](/home/andrekirst/git/github/andrekirst/family2/CLAUDE.md) - Single file, 200 lines
2. **Workflows:** [WORKFLOWS.md](/home/andrekirst/git/github/andrekirst/family2/docs/development/WORKFLOWS.md) - Common tasks
3. **Patterns:** [PATTERNS.md](/home/andrekirst/git/github/andrekirst/family2/docs/development/PATTERNS.md) - DDD patterns
4. **Standards:** THIS FILE - Comprehensive coding standards
5. **Deep Dive:** 52 docs in `/docs/` - Full context

**AI Agent Workflow:**

```
1. User requests feature
2. Claude reads CLAUDE.md (Quick context)
3. Spawns code-explorer (Find existing patterns)
4. Spawns code-architect (Design solution)
5. Implements following EXACT patterns
6. Runs tests, formats code
7. Commits with co-author attribution
```

**See:** [IMPLEMENTATION_WORKFLOW.md](IMPLEMENTATION_WORKFLOW.md)

---

### 34. Quality Gates

**Decision:** Automated quality gates + periodic manual reviews.

#### ✅ Pre-Commit Hooks

```bash
# .husky/pre-commit
npm run lint              # ESLint + Prettier
dotnet format --verify-no-changes  # C# formatting
dotnet test               # Unit tests
npm run test:unit         # Jest tests
```

#### ✅ CI/CD Pipeline

```yaml
# .github/workflows/ci.yml
name: CI

on: [push, pull_request]

jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Build
        run: dotnet build
      - name: Test
        run: dotnet test --collect:"XPlat Code Coverage"
      - name: Code Coverage
        uses: codecov/codecov-action@v3
        with:
          fail_ci_if_error: true
          threshold: 80%

  frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Install
        run: npm ci
      - name: Lint
        run: npm run lint
      - name: Test
        run: npm run test:ci
      - name: E2E
        run: npm run e2e:ci
```

#### ✅ Manual Review Checklist

**Every 2 weeks:**

- [ ] Code review: Adherence to standards
- [ ] Architecture review: Module boundaries respected
- [ ] Performance review: Query optimization
- [ ] Security review: Auth/authz patterns
- [ ] Documentation review: Up-to-date ADRs

---

## Anti-Patterns to Avoid

### ❌ Common Mistakes

#### 1. God Aggregates

```csharp
// ❌ DON'T: Aggregate with too many responsibilities
public class Family : AggregateRoot<FamilyId>
{
    public List<FamilyMemberInvitation> Invitations { get; set; }  // ❌
    public List<CalendarEvent> Events { get; set; }                // ❌
    public List<Task> Tasks { get; set; }                          // ❌
    public ShoppingList ShoppingList { get; set; }                 // ❌
    public Budget Budget { get; set; }                             // ❌
    // This is a maintenance nightmare
}

// ✅ DO: Small, focused aggregates
public class Family : AggregateRoot<FamilyId>
{
    public FamilyName Name { get; private set; }
    public UserId OwnerId { get; private set; }
    private List<User> _members = new();
    // Family data ONLY
}
```

#### 2. Anemic Domain Model

```csharp
// ❌ DON'T: Entities with only getters/setters (no behavior)
public class Family
{
    public Guid Id { get; set; }
    public string Name { get; set; }  // ❌ Public setter
    public List<User> Members { get; set; }  // ❌ Direct mutation
}

// Business logic in service layer ❌
public class FamilyService
{
    public void Rename(Family family, string newName)
    {
        family.Name = newName;  // ❌ Logic outside domain
    }
}

// ✅ DO: Rich domain model with behavior
public class Family : AggregateRoot<FamilyId>
{
    public FamilyName Name { get; private set; }  // ✅ Private setter

    public void Rename(FamilyName newName)  // ✅ Behavior method
    {
        if (Name == newName) return;
        Name = newName;
        RaiseDomainEvent(new FamilyRenamedEvent(Id, Name, newName));
    }
}
```

#### 3. Service Layer Business Logic

```csharp
// ❌ DON'T: Business rules in application layer
public class CreateFamilyCommandHandler
{
    public async Task<CreateFamilyResult> Handle(CreateFamilyCommand request)
    {
        var family = new Family();
        family.Id = Guid.NewGuid();  // ❌
        family.Name = request.Name;  // ❌
        family.OwnerId = _userContext.UserId;  // ❌
        family.CreatedAt = DateTime.UtcNow;  // ❌

        // ❌ Business rules in handler
        if (await _repository.ExistsByNameAsync(request.Name))
            return new CreateFamilyResult { Error = "..." };
    }
}

// ✅ DO: Business logic in domain
public class CreateFamilyCommandHandler
{
    public async Task<CreateFamilyResult> Handle(CreateFamilyCommand request)
    {
        // ✅ Domain factory method encapsulates creation
        var family = Family.Create(request.Name, _userContext.UserId);

        await _repository.AddAsync(family);
        return new CreateFamilyResult { Family = family };
    }
}

public class Family
{
    public static Family Create(FamilyName name, UserId ownerId)
    {
        // ✅ Business rules in domain
        var family = new Family
        {
            Id = FamilyId.New(),
            Name = name,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };

        family.RaiseDomainEvent(new FamilyCreatedEvent(family.Id, name, ownerId));
        return family;
    }
}
```

#### 4. Leaky Abstractions

```csharp
// ❌ DON'T: Domain layer depends on infrastructure
public interface IFamilyRepository
{
    Task<Family?> GetByIdAsync(FamilyId id, CancellationToken ct);
    IQueryable<Family> GetQueryable();  // ❌ Exposes EF Core
}

// ✅ DO: Pure domain interface
public interface IFamilyRepository
{
    Task<Family?> GetByIdAsync(FamilyId id, CancellationToken ct);
    Task<List<Family>> GetByOwnerIdAsync(UserId ownerId, CancellationToken ct);
    Task AddAsync(Family family, CancellationToken ct);
}
```

#### 5. Over-Engineering

```csharp
// ❌ DON'T: Unnecessary abstraction layers
public interface IFamilyNameValidator { }
public interface IFamilyNameFormatter { }
public interface IFamilyFactory { }
public interface IFamilyDomainService { }
// Too many layers for simple domain

// ✅ DO: Keep it simple
public class Family
{
    public static Family Create(FamilyName name, UserId ownerId) { }
    public void Rename(FamilyName newName) { }
}
```

#### 6. Ignoring Domain Events

```csharp
// ❌ DON'T: Direct cross-aggregate calls
public class FamilyService
{
    public async Task CreateFamily(CreateFamilyCommand cmd)
    {
        var family = Family.Create(cmd.Name, cmd.OwnerId);
        await _familyRepository.AddAsync(family);

        // ❌ Direct call to other service (tight coupling)
        await _calendarService.CreateDefaultCalendar(family.Id);
        await _taskService.CreateDefaultTaskList(family.Id);
    }
}

// ✅ DO: Use domain events for cross-aggregate coordination
public class Family
{
    public static Family Create(FamilyName name, UserId ownerId)
    {
        var family = new Family { /* ... */ };

        // ✅ Raise event - other services react
        family.RaiseDomainEvent(new FamilyCreatedEvent(family.Id, name, ownerId));

        return family;
    }
}
```

#### 7. Mocking Domain Logic

```csharp
// ❌ DON'T: Mock domain objects in tests
[Fact]
public void Family_Rename_RaisesEvent()
{
    var familyMock = new Mock<Family>();  // ❌ Mocking domain entity
    familyMock.Setup(f => f.Rename(It.IsAny<FamilyName>()));
}

// ✅ DO: Test real domain objects
[Fact]
public void Family_Rename_RaisesEvent()
{
    var family = Family.Create(FamilyName.From("Smith"), UserId.New());

    family.Rename(FamilyName.From("Johnson"));

    family.DomainEvents.Should().ContainSingle(e => e is FamilyRenamedEvent);
}
```

#### 8. Primitive Obsession

```csharp
// ❌ DON'T: Using primitives for domain concepts
public class Family
{
    public string Name { get; set; }  // ❌ What validation? Max length?
    public Guid OwnerId { get; set; }  // ❌ Is this a UserId or FamilyId?
}

// ✅ DO: Use value objects
public class Family
{
    public FamilyName Name { get; private set; }  // ✅ Validated value object
    public UserId OwnerId { get; private set; }  // ✅ Type-safe ID
}
```

---

## When to Deviate

**These standards are guidelines, not laws.** Deviate when:

### ✅ Acceptable Deviations

1. **Performance Critical Code**

   - Example: Inline validation instead of Vogen for hot paths
   - Rationale: 10x performance gain, profiler data confirms
   - Document: Add comment explaining why

2. **Third-Party Library Constraints**

   - Example: Using primitives for library DTOs
   - Rationale: Library doesn't support Vogen
   - Document: Link to library issue/documentation

3. **Prototyping/Spikes**

   - Example: Skipping tests for proof-of-concept
   - Rationale: Exploring feasibility, code will be rewritten
   - Document: Mark with `// TODO: Spike code - rewrite before merge`

4. **Legacy Code Interop**
   - Example: Not using primary constructors in old code
   - Rationale: Mixing patterns in same file reduces readability
   - Document: Refactor entire file when touching it

### ❌ Never Deviate On

1. **Security/Authorization:** ALWAYS follow auth patterns
2. **Event Idempotency:** NEVER skip event ID checks
3. **Domain Event Publishing:** ALWAYS raise domain events for state changes
4. **FluentAssertions:** ALWAYS use (no xUnit Assert)
5. **Logger Source Generation:** ALWAYS use (no manual ILogger calls)

**When in doubt:** Ask for review in PR, reference this document.

---

## Enforcement

### Pre-Commit

- Prettier + ESLint (TypeScript)
- dotnet format (C#)
- Unit tests must pass

### CI/CD

- All tests must pass (unit, integration, E2E)
- Code coverage ≥ 80%
- No linting errors
- Build succeeds

### PR Review

- Architectural patterns followed
- Tests included
- Documentation updated (if needed)
- ADRs referenced (if architectural change)

### Periodic Review

- Every 2 weeks: Manual code review
- Every sprint: Architecture review
- Every month: Update this document

---

## Version History

- **v1.0.0** (2026-01-07): Initial comprehensive coding standards
  - Based on interview results and codebase analysis
  - Covers C#, TypeScript, DDD, GraphQL, Testing, Security
  - Includes 34 standards with examples and anti-patterns

---

**Last Updated:** 2026-01-07
**Status:** Living Document - Updated as patterns evolve
**Maintainer:** Development Team + Claude Code

**Related Documents:**

- [CLAUDE.md](/home/andrekirst/git/github/andrekirst/family2/CLAUDE.md) - Quick reference for Claude Code
- [WORKFLOWS.md](WORKFLOWS.md) - Development workflow details
- [PATTERNS.md](PATTERNS.md) - DDD pattern examples
- [IMPLEMENTATION_WORKFLOW.md](IMPLEMENTATION_WORKFLOW.md) - Feature implementation process
- [ADR Index](../architecture/) - Architecture decision records
