# Standards for Event Chain Engine

The following standards apply to this work and guide our implementation decisions.

---

## 1. backend/domain-events

**Source**: `agent-os/standards/backend/domain-events.md`

# Domain Events

Use MediatR INotification for domain events. Publish via RabbitMQ for cross-module communication.

## Event Definition

```csharp
public sealed record FamilyCreatedEvent(
    FamilyId FamilyId,
    FamilyName Name,
    UserId OwnerId,
    DateTime CreatedAt
) : INotification;
```

## Publishing (in aggregate)

```csharp
public class Family : AggregateRoot
{
    public static Family Create(FamilyName name, UserId ownerId)
    {
        var family = new Family
        {
            Id = FamilyId.New(),
            Name = name,
            OwnerId = ownerId
        };

        family.AddDomainEvent(new FamilyCreatedEvent(
            family.Id,
            family.Name,
            family.OwnerId,
            DateTime.UtcNow
        ));

        return family;
    }
}
```

## Handler

```csharp
public sealed class SendWelcomeEmailHandler
    : INotificationHandler<FamilyCreatedEvent>
{
    public async Task Handle(
        FamilyCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        // Handle event
    }
}
```

## Cross-Module Events (RabbitMQ)

```csharp
await _messageBroker.PublishAsync(
    new FamilyCreatedEvent(family.Id, family.Name, ownerId, DateTime.UtcNow),
    cancellationToken);
```

## Rules

- Events are immutable records
- Location: `Domain/Events/{Name}Event.cs`
- Handlers: `Infrastructure/EventHandlers/{Name}Handler.cs`
- Use past tense: `Created`, `Updated`, `Deleted`

**Note**: Actual codebase uses Wolverine `IMessageBus` (not MediatR). Standard is aspirational — follow actual codebase patterns.

---

## 2. architecture/event-chains

**Source**: `agent-os/standards/architecture/event-chains.md`

# Event Chain Automation

Family Hub's flagship differentiator. Automated cross-domain workflows that save 10-30 minutes per action.

## Doctor Appointment Chain Example

```
1. User schedules doctor appointment (Health)
   └→ DoctorAppointmentScheduledEvent
        ├→ Calendar: Creates calendar event
        ├→ Task: Creates "Prepare questions" task
        └→ Communication: Schedules 24h reminder

2. Doctor issues prescription (Health)
   └→ PrescriptionIssuedEvent
        ├→ Shopping: Adds medication to list
        ├→ Task: Creates "Pick up prescription" task
        └→ Health: Schedules refill reminder
```

## Event Handler Pattern

```csharp
public class CreateCalendarEventHandler
    : INotificationHandler<DoctorAppointmentScheduledEvent>
{
    private readonly ICalendarService _calendar;

    public async Task Handle(
        DoctorAppointmentScheduledEvent notification,
        CancellationToken cancellationToken)
    {
        await _calendar.CreateEventAsync(new CalendarEvent
        {
            Title = $"Doctor: {notification.DoctorName}",
            StartTime = notification.AppointmentTime,
            FamilyId = notification.FamilyId
        });
    }
}
```

## 10 Documented Event Chains

1. Doctor Appointment → Calendar + Task + Reminder
2. School Event → Calendar + Task + Shopping
3. Meal Plan → Shopping List + Calendar
4. Prescription → Shopping + Task + Refill Reminder
5. Birthday → Calendar + Task + Shopping
6. Bill Due → Finance + Task + Reminder
7. Vacation → Calendar + Task + Shopping
8. Grocery Low → Shopping + Meal Adjustment
9. Family Meeting → Calendar + Task + Notification
10. Health Checkup → Calendar + Health Record

## Rules

- Events flow through RabbitMQ
- Handlers are idempotent
- Failed handlers retry with exponential backoff
- Dead letter queue for permanent failures

---

## 3. architecture/ddd-modules

**Source**: `agent-os/standards/architecture/ddd-modules.md`

# DDD Module Structure

Modular monolith with 8 bounded contexts. Each module is self-contained.

## Module Layout

```
Modules/FamilyHub.Modules.{ModuleName}/
├── Domain/
│   ├── Entities/          # Aggregates
│   ├── ValueObjects/      # Vogen types
│   ├── Events/            # Domain events
│   └── Repositories/      # Repository interfaces
├── Application/
│   ├── Commands/          # Write operations
│   ├── Queries/           # Read operations
│   ├── Handlers/          # MediatR handlers
│   └── Validators/        # FluentValidation
├── Persistence/
│   ├── Configurations/    # EF Core configs
│   ├── Repositories/      # Implementations
│   └── Migrations/        # EF Core migrations
└── Presentation/
    ├── GraphQL/           # Mutations, queries, types
    └── DTOs/              # Input DTOs
```

## 8 Domain Modules

| Module | Schema | Aggregates |
|--------|--------|------------|
| Auth | auth | User, Family |
| Calendar | calendar | Event, Appointment |
| Task | task | Task, Assignment |
| Shopping | shopping | ShoppingList, Item |
| Health | health | Appointment, Prescription |
| MealPlanning | meal | MealPlan, Recipe |
| Finance | finance | Budget, Expense |
| Communication | communication | Notification |

## Cross-Module Communication

- Use domain events via RabbitMQ
- Reference IDs only (no FK constraints across modules)
- IUserLookupService for cross-module queries

## Rules

- One DbContext per module
- One PostgreSQL schema per module
- No direct module dependencies
- Event-driven cross-module communication

---

## 4. database/ef-core-migrations

**Source**: `agent-os/standards/database/ef-core-migrations.md`

# EF Core Migrations

One DbContext per module, each targeting its own PostgreSQL schema.

## Create Migration

```bash
dotnet ef migrations add MigrationName \
  --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api \
  --output-dir Persistence/Migrations
```

## Apply Migration

```bash
# Development
dotnet ef database update --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api

# Production (in Program.cs)
await context.Database.MigrateAsync();
```

## Schema Separation

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.HasDefaultSchema("auth");  // Each module has its own schema
}
```

## PostgreSQL RLS

```csharp
// In migration Up() method
migrationBuilder.Sql(@"
    ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

    CREATE POLICY user_isolation_policy ON auth.users
        USING (id = current_setting('app.current_user_id')::uuid);
");
```

## Rules

- Migration name format: `{Timestamp}_{Description}`
- Always test down migrations
- One DbContext per module
- Schema name = module name (lowercase)
- Enable RLS on tenant-isolated tables

---

## 5. database/rls-policies

**Source**: `agent-os/standards/database/rls-policies.md`

# PostgreSQL Row-Level Security

RLS enforces data isolation at database level. Defense in depth for multi-tenancy.

## Enable RLS

```sql
ALTER TABLE auth.families ENABLE ROW LEVEL SECURITY;
```

## User Isolation Policy

```sql
CREATE POLICY user_isolation_policy ON auth.users
    USING (id = current_setting('app.current_user_id', true)::uuid);
```

## Family Isolation Policy

```sql
CREATE POLICY family_isolation_policy ON auth.family_members
    USING (family_id = current_setting('app.current_family_id', true)::uuid);
```

## Middleware Setup

```csharp
// PostgresRlsContextMiddleware
app.Use(async (context, next) =>
{
    var userId = context.User.FindFirstValue("sub");
    await dbConnection.ExecuteAsync(
        "SET app.current_user_id = @userId",
        new { userId });
    await next();
});
```

## Rules

- Enable RLS on all tenant-specific tables
- Use `current_setting(..., true)` for safe NULL handling
- Set session variables in middleware before queries
- Transaction-scoped variables for fail-secure behavior
- Test RLS with integration tests

---

## 6. backend/graphql-input-command

**Source**: `agent-os/standards/backend/graphql-input-command.md`

# GraphQL Input→Command Pattern

Separate Input DTOs (primitives) from MediatR Commands (Vogen). See ADR-003.

## Why

Hot Chocolate cannot deserialize Vogen value objects. This creates clean separation between presentation and domain layers.

## GraphQL Input (primitives only)

```csharp
public sealed record CreateFamilyInput
{
    [Required]
    public required string Name { get; init; }
}
```

## MediatR Command (Vogen types)

```csharp
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>;
```

## Mutation (mapping layer)

```csharp
public async Task<CreateFamilyPayload> CreateFamily(
    CreateFamilyInput input,
    [Service] IMediator mediator)
{
    var command = new CreateFamilyCommand(
        FamilyName.From(input.Name)  // Primitive → Vogen
    );
    var result = await mediator.Send(command);
    return new CreateFamilyPayload(result);
}
```

## Rules

- Input DTOs: `Presentation/DTOs/{Name}Input.cs`
- Commands: `Application/Commands/{Name}Command.cs`
- Handlers: `Application/Handlers/{Name}CommandHandler.cs`
- Never use Vogen types in GraphQL input types
- Always validate at Vogen boundary (`.From()` throws if invalid)

---

## 7. backend/vogen-value-objects

**Source**: `agent-os/standards/backend/vogen-value-objects.md`

# Vogen Value Objects

Always use Vogen 8.0+ for domain value objects. Never use primitives in commands/domain.

## Definition Pattern

```csharp
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email cannot be empty.");
        if (value.Length > 320)
            return Validation.Invalid("Email cannot exceed 320 characters.");
        if (!value.Contains('@'))
            return Validation.Invalid("Invalid email format.");
        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
        => input?.Trim().ToLowerInvariant() ?? string.Empty;
}
```

## Creation

```csharp
UserId userId = UserId.New();           // New GUID
Email email = Email.From("user@ex.com"); // With validation (throws if invalid)
Email.TryFrom("invalid", out var result); // Safe creation
```

## EF Core Configuration

```csharp
builder.Property(u => u.Email)
    .HasConversion(new Email.EfCoreValueConverter())
    .HasMaxLength(320)
    .IsRequired();
```

## Rules

- Always include `conversions: Conversions.EfCoreValueConverter`
- Implement `Validate()` for business rules
- Implement `NormalizeInput()` for string normalization
- Location: `Domain/ValueObjects/{Name}.cs`

---

## 8. testing/unit-testing

**Source**: `agent-os/standards/testing/unit-testing.md`

# Unit Testing (xUnit)

Use xUnit with FluentAssertions and NSubstitute.

## Test Structure

```csharp
public class CreateFamilyCommandHandlerTests
{
    private readonly IFamilyRepository _repository;
    private readonly CreateFamilyCommandHandler _handler;

    public CreateFamilyCommandHandlerTests()
    {
        _repository = Substitute.For<IFamilyRepository>();
        _handler = new CreateFamilyCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesFamily()
    {
        // Arrange
        var command = new CreateFamilyCommand(FamilyName.From("Smith"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FamilyId.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<Family>(f => f.Name.Value == "Smith"),
            Arg.Any<CancellationToken>());
    }
}
```

## AutoData Pattern

```csharp
public class AutoNSubstituteDataAttribute : AutoDataAttribute
{
    public AutoNSubstituteDataAttribute()
        : base(() => new Fixture().Customize(new AutoNSubstituteCustomization()))
    { }
}

[Theory, AutoNSubstituteData]
public async Task Handle_ValidCommand_CreatesFamily(
    [Frozen] IFamilyRepository repository,
    CreateFamilyCommandHandler handler,
    CreateFamilyCommand command)
{
    var result = await handler.Handle(command, CancellationToken.None);

    result.FamilyId.Should().NotBeEmpty();
}
```

## Rules

- Use FluentAssertions for readable assertions
- Use NSubstitute for mocking
- Arrange-Act-Assert pattern
- One assertion concept per test
- Location: `tests/FamilyHub.Tests.Unit/{Module}/`

---

## How These Standards Apply to Event Chain Engine

### Domain Events (Standard #1)

**Direct Application**: The chain engine subscribes to domain events published by other modules. Chain engine also publishes its own events (`ChainExecutionStartedEvent`, `ChainExecutionCompletedEvent`, `ChainExecutionFailedEvent`).

### Event Chains (Standard #2)

**This IS the standard**: The Event Chain Engine implements the automation pattern described in this standard. The 10 documented chains become the V1+ feature set.

### DDD Modules (Standard #3)

**Module Structure**: Event Chain Engine follows the module layout. Two aggregates (`ChainDefinition`, `ChainExecution`), dedicated `event_chain` schema, plugin registry for cross-module communication.

### EF Core Migrations (Standard #4)

**Schema Creation**: New `event_chain` schema with 6 tables. Migrations follow existing pattern in `src/FamilyHub.Api/Migrations/`.

### RLS Policies (Standard #5)

**Family Isolation**: All 6 tables in `event_chain` schema have RLS policies using `app.current_family_id` session variable.

### GraphQL Input→Command (Standard #6)

**API Layer**: Chain management mutations use Input DTOs (primitives) mapped to Commands (Vogen value objects) in mutation resolvers.

### Vogen Value Objects (Standard #7)

**Domain Types**: `ChainDefinitionId`, `ChainExecutionId`, `StepAlias`, `ActionVersion` — all Vogen value objects with validation.

### Unit Testing (Standard #8)

**Testing Priority**: State machine transitions are the #1 testing priority. FluentAssertions for readable tests, NSubstitute for mocking dependencies.
