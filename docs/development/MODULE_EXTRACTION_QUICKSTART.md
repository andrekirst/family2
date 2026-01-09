# Module Extraction Quick Start

**Purpose:** Step-by-step guide to extract a bounded context into its own module following DDD principles.

**Source:** Condensed from [ADR-005](docs/architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md) (1,520 lines → 300 actionable).

---

## Overview

**The 4-Phase Extraction Process:**

```
Phase 1: Domain Layer       (100% extracted - aggregates, value objects, events)
Phase 2: Application Layer  (Partial - commands/queries that own aggregates)
Phase 3: Persistence Layer  (Logical only - interfaces extracted, implementations stay)
Phase 4: Presentation Layer (Partial - GraphQL types/mutations that own aggregates)
```

**Core Principle:** Logical > Physical separation in modular monolith.

---

## Prerequisites

Before starting extraction:

1. **Identify bounded context:** Clear domain concept (e.g., Family, Calendar, Task)
2. **Locate aggregates:** Find entities that belong to this context
3. **Map dependencies:** Understand what references these aggregates
4. **Run tests:** Baseline test suite passing (create safety net)

---

## Phase 1: Domain Layer Extraction

**Goal:** Move all domain concepts (aggregates, value objects, events, interfaces) to new module.

### Step 1.1: Create Module Structure

```bash
cd src/api/Modules

# Create new module folder
mkdir -p FamilyHub.Modules.NewModule/{Domain/{Entities,ValueObjects,Events,Repositories},Application/{Commands,Queries,Handlers,Validators},Persistence/{Configurations,Repositories,Migrations},Presentation/{GraphQL/{Types,Mutations,Queries},DTOs}}
```

**Expected Structure:**

```
FamilyHub.Modules.NewModule/
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Events/
│   └── Repositories/        # Interfaces only
├── Application/
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   └── Validators/
├── Persistence/
│   ├── Configurations/      # EF Core configs
│   ├── Repositories/        # Implementations
│   └── Migrations/
└── Presentation/
    ├── GraphQL/
    │   ├── Types/
    │   ├── Mutations/
    │   └── Queries/
    └── DTOs/                # Input DTOs
```

### Step 1.2: Extract Aggregates

**Move aggregate root entities:**

```csharp
// From: FamilyHub.Modules.Auth/Domain/Family.cs
// To:   FamilyHub.Modules.Family/Domain/Entities/Family.cs

namespace FamilyHub.Modules.Family.Domain.Entities;

public sealed class Family : AggregateRoot<FamilyId>
{
    public FamilyName Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Factory method
    public static Family Create(FamilyName name)
    {
        var family = new Family
        {
            Id = FamilyId.New(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        // Raise domain event
        family.RaiseDomainEvent(new FamilyCreatedEvent(family.Id, family.Name));
        return family;
    }
}
```

### Step 1.3: Extract Value Objects

**Move Vogen value objects:**

```csharp
// From: FamilyHub.Modules.Auth/Domain/ValueObjects/FamilyName.cs
// To:   FamilyHub.Modules.Family/Domain/ValueObjects/FamilyName.cs

namespace FamilyHub.Modules.Family.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Family name cannot be empty.");

        if (value.Length > 100)
            return Validation.Invalid("Family name cannot exceed 100 characters.");

        return Validation.Ok;
    }

    private static string NormalizeInput(string input) => input.Trim();
}
```

### Step 1.4: Extract Domain Events

```csharp
// To: FamilyHub.Modules.Family/Domain/Events/FamilyCreatedEvent.cs

namespace FamilyHub.Modules.Family.Domain.Events;

public sealed record FamilyCreatedEvent(
    FamilyId FamilyId,
    FamilyName FamilyName
) : IDomainEvent;
```

### Step 1.5: Extract Repository Interfaces

```csharp
// To: FamilyHub.Modules.Family/Domain/Repositories/IFamilyRepository.cs

namespace FamilyHub.Modules.Family.Domain.Repositories;

public interface IFamilyRepository
{
    Task<Family?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default);
    Task AddAsync(Family family, CancellationToken cancellationToken = default);
    Task UpdateAsync(Family family, CancellationToken cancellationToken = default);
}
```

### Step 1.6: Update Namespaces

**Find and replace references:**

```bash
# Use IDE refactoring or grep
grep -r "FamilyHub.Modules.Auth.Domain.Family" src/api/
# Replace with: FamilyHub.Modules.Family.Domain.Entities.Family

# Update using statements in remaining Auth module files
```

### Step 1.7: Validate Phase 1

```bash
# Run tests
cd src/api
dotnet test

# Should see compilation errors in Auth module for missing references
# Fix by adding project reference:
cd Modules/FamilyHub.Modules.Auth
dotnet add reference ../FamilyHub.Modules.Family/FamilyHub.Modules.Family.csproj
```

**Checkpoint:** ✅ Domain layer fully extracted, tests passing.

---

## Phase 2: Application Layer Extraction

**Goal:** Move commands/queries that **own** Family aggregates. Keep commands that modify other aggregates.

### Step 2.1: Decision Matrix - What to Move?

| Command/Query | Owns Aggregate | Modifies Other Aggregate | Decision |
|---------------|----------------|--------------------------|----------|
| CreateFamilyCommand | Family | User (sets FamilyId) | ⚠️ **Stay in Auth** (modifies User) |
| InviteFamilyMemberCommand | FamilyInvitation | No | ✅ **Move to Family** |
| AcceptInvitationCommand | FamilyInvitation | User (sets FamilyId) | ⚠️ **Stay in Auth** (modifies User) |
| GetUserFamiliesQuery | Family | No | ✅ **Move to Family** |

**Rule:** If command modifies aggregate from **different** module, keep it in that module (aggregate ownership).

### Step 2.2: Extract Commands

```csharp
// To: FamilyHub.Modules.Family/Application/Commands/InviteFamilyMemberCommand.cs

namespace FamilyHub.Modules.Family.Application.Commands;

public sealed record InviteFamilyMemberCommand(
    FamilyId FamilyId,
    Email Email,
    FamilyRole Role
) : IRequest<InviteFamilyMemberResult>;
```

### Step 2.3: Extract Command Handlers

```csharp
// To: FamilyHub.Modules.Family/Application/Handlers/InviteFamilyMemberCommandHandler.cs

namespace FamilyHub.Modules.Family.Application.Handlers;

public sealed class InviteFamilyMemberCommandHandler
    : IRequestHandler<InviteFamilyMemberCommand, InviteFamilyMemberResult>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IFamilyInvitationRepository _invitationRepository;

    public async Task<InviteFamilyMemberResult> Handle(
        InviteFamilyMemberCommand command,
        CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Step 2.4: Extract Validators

```csharp
// To: FamilyHub.Modules.Family/Application/Validators/InviteFamilyMemberCommandValidator.cs

namespace FamilyHub.Modules.Family.Application.Validators;

public sealed class InviteFamilyMemberCommandValidator
    : AbstractValidator<InviteFamilyMemberCommand>
{
    public InviteFamilyMemberCommandValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();
    }
}
```

### Step 2.5: Extract Queries

```csharp
// To: FamilyHub.Modules.Family/Application/Queries/GetUserFamiliesQuery.cs

namespace FamilyHub.Modules.Family.Application.Queries;

public sealed record GetUserFamiliesQuery(
    UserId UserId
) : IRequest<IReadOnlyList<Family>>;

// Handler
public sealed class GetUserFamiliesQueryHandler
    : IRequestHandler<GetUserFamiliesQuery, IReadOnlyList<Family>>
{
    // Implementation
}
```

**Checkpoint:** ✅ Commands/queries that own Family aggregates extracted.

---

## Phase 3: Persistence Layer Extraction

**Goal:** Logical separation only. Interfaces in Family module, implementations stay in Auth (temporary coupling).

### Step 3.1: Repository Implementations (STAY in Auth)

```csharp
// Stays: FamilyHub.Modules.Auth/Persistence/Repositories/FamilyRepository.cs

using FamilyHub.Modules.Family.Domain.Repositories;  // Interface from Family module

namespace FamilyHub.Modules.Auth.Persistence.Repositories;

public sealed class FamilyRepository : IFamilyRepository  // Implements Family's interface
{
    private readonly AuthDbContext _context;

    public async Task<Family?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken)
    {
        return await _context.Families.FindAsync(new object[] { id }, cancellationToken);
    }
}
```

**Why keep implementations in Auth?**

- Auth module owns AuthDbContext (single database in modular monolith)
- Avoids circular dependencies (Family → Auth → Family)
- Pragmatic: Physical separation deferred to Phase 5+ microservices

### Step 3.2: EF Core Configurations

```csharp
// Move: FamilyHub.Modules.Family/Persistence/Configurations/FamilyConfiguration.cs

namespace FamilyHub.Modules.Family.Persistence.Configurations;

public sealed class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("families", "family");  // family schema
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasConversion(new FamilyId.EfCoreValueConverter())
            .IsRequired();

        builder.Property(f => f.Name)
            .HasConversion(new FamilyName.EfCoreValueConverter())
            .HasMaxLength(100)
            .IsRequired();
    }
}
```

### Step 3.3: Database Migrations (NEW DbContext)

**Create FamilyDbContext:**

```csharp
// To: FamilyHub.Modules.Family/Persistence/FamilyDbContext.cs

namespace FamilyHub.Modules.Family.Persistence;

public sealed class FamilyDbContext : DbContext
{
    public DbSet<Family> Families => Set<Family>();

    public FamilyDbContext(DbContextOptions<FamilyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("family");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FamilyDbContext).Assembly);
    }
}
```

**Register in Program.cs:**

```csharp
builder.Services.AddDbContext<FamilyDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "family")));
```

**Create initial migration:**

```bash
dotnet ef migrations add InitialCreate \
  --context FamilyDbContext \
  --project Modules/FamilyHub.Modules.Family \
  --startup-project FamilyHub.Api \
  --output-dir Persistence/Migrations
```

**Checkpoint:** ✅ Logical persistence separation complete.

---

## Phase 4: Presentation Layer Extraction

**Goal:** Move GraphQL types/mutations that **own** Family aggregates.

### Step 4.1: Extract GraphQL Types

```csharp
// To: FamilyHub.Modules.Family/Presentation/GraphQL/Types/FamilyType.cs

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Types;

public sealed class FamilyType : ObjectType<Family>
{
    protected override void Configure(IObjectTypeDescriptor<Family> descriptor)
    {
        descriptor.Field(f => f.Id).Type<NonNullType<IdType>>();
        descriptor.Field(f => f.Name).Type<NonNullType<StringType>>();
        descriptor.Field(f => f.CreatedAt).Type<NonNullType<DateTimeType>>();
    }
}
```

### Step 4.2: Extract Input DTOs

```csharp
// To: FamilyHub.Modules.Family/Presentation/DTOs/InviteFamilyMemberInput.cs

namespace FamilyHub.Modules.Family.Presentation.DTOs;

public sealed record InviteFamilyMemberInput
{
    [Required]
    public required string FamilyId { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Role { get; init; }
}
```

### Step 4.3: Extract Mutations

```csharp
// To: FamilyHub.Modules.Family/Presentation/GraphQL/Mutations/FamilyMutations.cs

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Mutations;

public sealed class FamilyMutations
{
    public async Task<InviteFamilyMemberPayload> InviteFamilyMember(
        InviteFamilyMemberInput input,
        [Service] IMediator mediator)
    {
        var command = new InviteFamilyMemberCommand(
            FamilyId.From(input.FamilyId),
            Email.From(input.Email),
            Enum.Parse<FamilyRole>(input.Role)
        );

        var result = await mediator.Send(command);
        return new InviteFamilyMemberPayload(result);
    }
}
```

### Step 4.4: Register GraphQL Types

```csharp
// Program.cs
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<FamilyType>()          // Register Family types
    .AddType<FamilyMutations>();    // Register Family mutations
```

**Checkpoint:** ✅ Presentation layer extraction complete.

---

## Common Pitfalls & Solutions

### Pitfall 1: Circular Dependencies

**Problem:** Family module references Auth, Auth references Family.

**Solution:**

- Use SharedKernel for common value objects (UserId, Email)
- Family should NOT reference Auth module
- Auth can reference Family (temporary, until Phase 5)

### Pitfall 2: Commands Spanning Aggregates

**Problem:** CreateFamilyCommand modifies both Family and User aggregates.

**Solution:**

- Keep command in Auth (owns User aggregate)
- Publish FamilyCreatedEvent
- Use event-driven integration instead of direct coupling

### Pitfall 3: Broken Tests After Extraction

**Problem:** Tests fail due to missing dependencies.

**Solution:**

```bash
# Update test project references
cd tests/FamilyHub.Tests.Unit
dotnet add reference ../../src/api/Modules/FamilyHub.Modules.Family/FamilyHub.Modules.Family.csproj

# Update using statements
# Replace: using FamilyHub.Modules.Auth.Domain;
# With:    using FamilyHub.Modules.Family.Domain.Entities;
```

### Pitfall 4: Database Schema Conflicts

**Problem:** Two modules trying to manage same table.

**Solution:**

- Each module owns its schema (e.g., `auth.users`, `family.families`)
- Use PostgreSQL schemas for separation
- Never cross-schema FK constraints (use IDs only)

---

## Validation Checklist

After completing extraction, verify:

- [ ] All tests passing (`dotnet test`)
- [ ] No circular dependencies (check `.csproj` files)
- [ ] GraphQL schema builds (`dotnet run`)
- [ ] Database migrations apply (`dotnet ef database update`)
- [ ] Domain events published correctly
- [ ] No Auth module imports in Family domain layer
- [ ] Repository interfaces owned by Family
- [ ] Aggregate ownership respected

---

## Next Steps After Extraction

1. **Run full test suite:** Ensure no regressions
2. **Test GraphQL mutations:** Verify extracted mutations work
3. **Check domain events:** Confirm event-driven integration works
4. **Update documentation:** Document new module boundaries
5. **Plan next extraction:** Apply pattern to next bounded context

---

## When to Do Physical Separation (Phase 5+)

**Defer physical separation until:**

- [ ] All 8 modules extracted logically
- [ ] Event-driven integration mature
- [ ] Microservices migration begins (Phase 5)
- [ ] Operational complexity justified (K8s, separate deployments)

**Physical separation includes:**

- Separate databases per module
- Separate deployments
- Network-based event bus (RabbitMQ)
- API gateways
- Service mesh

---

## Related Documentation

- **Full Pattern:** [ADR-005](docs/architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md) (1,520 lines)
- **Modular Monolith:** [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)
- **DDD Patterns:** [PATTERNS.md](PATTERNS.md)
- **Workflows:** [WORKFLOWS.md](WORKFLOWS.md)

---

**Last Updated:** 2026-01-09
**Version:** 1.0.0
**Based on:** ADR-005 (Family Module Extraction)
