# Backend Development Guide

**Purpose:** Guide for developing Family Hub's .NET backend API with GraphQL, DDD, and event-driven architecture.

**Tech Stack:** .NET Core 10, C# 14, Hot Chocolate GraphQL 14.1.0, EF Core 10, PostgreSQL 16, Vogen 8.0+, MediatR 12.4.1, RabbitMQ

---

## Quick Reference

### Module Structure

```
Modules/FamilyHub.Modules.Auth/
├── Domain/
│   ├── Entities/          # Aggregates (User, Family)
│   ├── ValueObjects/      # Vogen types (UserId, Email)
│   ├── Events/            # Domain events
│   └── Repositories/      # Repository interfaces
├── Application/
│   ├── Commands/          # Write operations
│   ├── Queries/           # Read operations
│   ├── Handlers/          # MediatR handlers
│   └── Validators/        # FluentValidation
├── Persistence/
│   ├── Configurations/    # EF Core configs
│   ├── Repositories/      # Repository implementations
│   └── Migrations/        # EF Core migrations
└── Presentation/
    ├── GraphQL/           # Mutations, queries, types
    └── DTOs/              # Input DTOs (primitives)
```

---

## Critical Patterns (4)

### 1. EF Core Migrations

**One DbContext per module**, each targeting its own PostgreSQL schema.

**Create Migration:**

```bash
dotnet ef migrations add MigrationName \
  --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api \
  --output-dir Persistence/Migrations
```

**Apply Migration:**

```bash
# Development
dotnet ef database update --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api

# Production (in Program.cs)
await context.Database.MigrateAsync();
```

**Vogen Integration:**

```csharp
// IEntityTypeConfiguration<User>
builder.Property(u => u.Id)
    .HasConversion(new UserId.EfCoreValueConverter())
    .IsRequired();

builder.Property(u => u.Email)
    .HasConversion(new Email.EfCoreValueConverter())
    .HasMaxLength(320)
    .IsRequired();
```

**PostgreSQL RLS Policies:**

```csharp
// In migration Up() method
migrationBuilder.Sql(@"
    ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

    CREATE POLICY user_isolation_policy ON auth.users
        USING (id = current_setting('app.current_user_id')::uuid);
");
```

---

### 2. Vogen Value Objects

**Always use Vogen** for domain value objects (never primitives in commands/domain).

**Example:**

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

**Creation:**

```csharp
// New GUID
UserId userId = UserId.New();

// With validation
Email email = Email.From("user@example.com");  // Throws if invalid
Email.TryFrom("invalid", out var result);      // Safe creation

// In tests
var testEmail = Email.From("test@example.com");
```

**EF Core Configuration:**

```csharp
builder.Property(u => u.Email)
    .HasConversion(new Email.EfCoreValueConverter())
    .HasMaxLength(320)
    .IsRequired();
```

---

### 3. GraphQL Input→Command Pattern

**Separate Input DTOs (primitives) from MediatR Commands (Vogen)**. See [ADR-003](../../docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md).

**GraphQL Input (primitives):**

```csharp
public sealed record CreateFamilyInput
{
    [Required]
    public required string Name { get; init; }
}
```

**MediatR Command (Vogen):**

```csharp
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>;
```

**GraphQL Mutation (mapping):**

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

**Command Handler:**

```csharp
public sealed class CreateFamilyCommandHandler
    : IRequestHandler<CreateFamilyCommand, CreateFamilyResult>
{
    private readonly IFamilyRepository _repository;

    public async Task<CreateFamilyResult> Handle(
        CreateFamilyCommand command,
        CancellationToken cancellationToken)
    {
        var family = Family.Create(command.Name);
        await _repository.AddAsync(family, cancellationToken);
        return new CreateFamilyResult(family.Id, family.Name);
    }
}
```

**FluentValidation:**

```csharp
public sealed class CreateFamilyCommandValidator
    : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}
```

---

### 4. Domain Events (RabbitMQ)

**Publish events** from aggregates, **subscribe** via MediatR handlers.

**Define Event:**

```csharp
public sealed record FamilyCreatedEvent(
    FamilyId FamilyId,
    FamilyName FamilyName,
    DateTime CreatedAt
) : IDomainEvent;
```

**Raise in Aggregate:**

```csharp
public sealed class Family : AggregateRoot<FamilyId>
{
    public static Family Create(FamilyName name)
    {
        var family = new Family
        {
            Id = FamilyId.New(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        family.RaiseDomainEvent(new FamilyCreatedEvent(
            family.Id,
            family.Name,
            family.CreatedAt
        ));

        return family;
    }
}
```

**Event Handler:**

```csharp
public sealed class FamilyCreatedEventHandler
    : INotificationHandler<FamilyCreatedEvent>
{
    private readonly IMessageBrokerPublisher _publisher;

    public async Task Handle(
        FamilyCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        // Publish to RabbitMQ for event chain automation
        await _publisher.PublishAsync(notification, cancellationToken);
    }
}
```

---

## Testing Patterns

### Unit Tests (xUnit + FluentAssertions)

**Always use:**

- `[Theory, AutoNSubstituteData]` for tests with dependencies
- `FluentAssertions` for ALL assertions (never xUnit Assert)
- Create Vogen value objects manually

**Example:**

```csharp
[Theory, AutoNSubstituteData]
public async Task Handle_ValidCommand_CreatesFamily(
    [Frozen] Mock<IFamilyRepository> repositoryMock,
    CreateFamilyCommandHandler sut,
    FamilyName familyName)
{
    // Arrange
    var command = new CreateFamilyCommand(familyName);
    repositoryMock.Setup(r => r.AddAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.FamilyId.Should().NotBeEmpty();
    repositoryMock.Verify(r => r.AddAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### Integration Tests

**Test with real DbContext:**

```csharp
public class FamilyRepositoryTests : IAsyncLifetime
{
    private AuthDbContext _context;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AuthDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingFamily_ReturnsFamily()
    {
        // Arrange
        var family = Family.Create(FamilyName.From("Test Family"));
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync();

        var repository = new FamilyRepository(_context);

        // Act
        var result = await repository.GetByIdAsync(family.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(FamilyName.From("Test Family"));
    }
}
```

---

## Common Backend Tasks

### Add New GraphQL Mutation

1. Create Input DTO (primitives):

```csharp
public sealed record UpdateFamilyNameInput
{
    [Required] public required string FamilyId { get; init; }
    [Required] public required string Name { get; init; }
}
```

1. Create Command (Vogen):

```csharp
public sealed record UpdateFamilyNameCommand(
    FamilyId FamilyId,
    FamilyName Name
) : IRequest<UpdateFamilyNameResult>;
```

1. Create Handler:

```csharp
public sealed class UpdateFamilyNameCommandHandler
    : IRequestHandler<UpdateFamilyNameCommand, UpdateFamilyNameResult>
{
    // Implementation
}
```

1. Create Mutation:

```csharp
public async Task<UpdateFamilyNamePayload> UpdateFamilyName(
    UpdateFamilyNameInput input,
    [Service] IMediator mediator)
{
    var command = new UpdateFamilyNameCommand(
        FamilyId.From(input.FamilyId),
        FamilyName.From(input.Name)
    );
    var result = await mediator.Send(command);
    return new UpdateFamilyNamePayload(result);
}
```

### Add New Domain Event

1. Define event:

```csharp
public sealed record FamilyNameUpdatedEvent(
    FamilyId FamilyId,
    FamilyName OldName,
    FamilyName NewName
) : IDomainEvent;
```

1. Raise in aggregate:

```csharp
public void UpdateName(FamilyName newName)
{
    var oldName = Name;
    Name = newName;
    RaiseDomainEvent(new FamilyNameUpdatedEvent(Id, oldName, newName));
}
```

1. Create handler:

```csharp
public sealed class FamilyNameUpdatedEventHandler
    : INotificationHandler<FamilyNameUpdatedEvent>
{
    // Implementation
}
```

---

## Module Extraction

**When extracting bounded contexts:** Follow [MODULE_EXTRACTION_QUICKSTART.md](../../docs/development/MODULE_EXTRACTION_QUICKSTART.md).

**Quick Checklist:**

- [ ] Phase 1: Extract domain layer (aggregates, value objects, events)
- [ ] Phase 2: Extract application layer (commands/queries that own aggregates)
- [ ] Phase 3: Logical persistence separation (interfaces in new module)
- [ ] Phase 4: Extract presentation layer (GraphQL types/mutations)
- [ ] Validate: No circular dependencies, tests passing

---

## Debugging

**Common Issues:**

- **Vogen validation failed:** Check validation rules in value object
- **Migration errors:** Verify connection string, DbContext name
- **GraphQL schema errors:** Check type registrations in Program.cs
- **RabbitMQ connection refused:** Verify Docker container running

**See:** [DEBUGGING_GUIDE.md](../../docs/development/DEBUGGING_GUIDE.md)

---

## Educational Insights

**Backend-Specific Examples:**

```
★ Insight ─────────────────────────────────────
1. EF Core migrations with Vogen require explicit value converters
2. PostgreSQL RLS policies enforce multi-tenant isolation at DB level
3. One DbContext per module enforces bounded context boundaries
─────────────────────────────────────────────────
```

```
★ Insight ─────────────────────────────────────
1. GraphQL Input→Command separation enables clean validation layers
2. Input DTOs use primitives for JSON deserialization
3. Commands use Vogen value objects for domain correctness
─────────────────────────────────────────────────
```

```
★ Insight ─────────────────────────────────────
1. Domain events enable loose coupling between modules
2. RabbitMQ provides reliable event delivery for event chains
3. Event-driven architecture supports eventual consistency
─────────────────────────────────────────────────
```

---

## Related Documentation

- **Coding Standards:** [CODING_STANDARDS.md](../../docs/development/CODING_STANDARDS.md)
- **Workflows:** [WORKFLOWS.md](../../docs/development/WORKFLOWS.md)
- **Patterns:** [PATTERNS.md](../../docs/development/PATTERNS.md)
- **ADR-003:** [GraphQL Input→Command Pattern](../../docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)
- **ADR-005:** [Module Extraction](../../docs/architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md)

---

**Last Updated:** 2026-01-09
**Derived from:** Root CLAUDE.md v5.0.0
**Canonical Sources:**

- docs/development/WORKFLOWS.md (EF Core migrations, Vogen patterns, GraphQL)
- docs/development/PATTERNS.md (Domain events, aggregates)
- docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md

**Sync Checklist:**

- [ ] EF Core migration commands match WORKFLOWS.md
- [ ] Vogen patterns match WORKFLOWS.md#value-objects
- [ ] GraphQL Input→Command matches ADR-003
- [ ] Domain event patterns match PATTERNS.md
