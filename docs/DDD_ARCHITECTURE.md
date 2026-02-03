# DDD Architecture Guide

**Status**: Active (Phase 0 - Issue #113 Restored)
**Date**: 2026-02-02
**Supersedes**: Simplified architecture from commit #112

---

## Overview

Family Hub uses a **Domain-Driven Design (DDD) architecture within a single project** to enable event-driven automation while maintaining simplicity. This architecture supports the strategic differentiator of **event chain automation** through proper aggregates, domain events, and CQRS patterns.

**Key Characteristics:**

- ✅ **Single FamilyHub.Api project** (not separate microservices)
- ✅ **DDD patterns** (aggregates, value objects, domain events)
- ✅ **CQRS with Wolverine** (command/query separation)
- ✅ **Event-driven architecture** (foundation for event chains)
- ✅ **Type safety with Vogen** (value objects prevent invalid states)

---

## Project Structure

> **Note:** Solution file is co-located with the API project at `src/FamilyHub.Api/FamilyHub.sln`.
> **Architecture:** DDD layers organized as folders within single project (not separate assemblies).

```
repository-root/
├── src/
│   └── FamilyHub.Api/                          # Single API project (DDD layers as folders)
│       ├── FamilyHub.sln                       # Solution file (co-located)
│       ├── FamilyHub.Api.csproj
│       ├── Features/                           # Feature-based modules
│       │   ├── Auth/                           # Auth bounded context
│       │   │   ├── Domain/                     # Domain layer
│       │   │   │   ├── Entities/               # Aggregates (User)
│       │   │   │   ├── ValueObjects/           # Value objects (UserId, UserName, etc.)
│       │   │   │   ├── Events/                 # Domain events
│       │   │   │   └── Repositories/           # Repository interfaces
│       │   │   ├── Application/                # Application layer
│       │   │   │   ├── Commands/               # Write operations
│       │   │   │   ├── Queries/                # Read operations
│       │   │   │   ├── Handlers/               # Command/Query handlers
│       │   │   │   ├── Validators/             # FluentValidation
│       │   │   │   ├── EventHandlers/          # Domain event handlers
│       │   │   │   └── Mappers/                # DTO mappers
│       │   │   ├── Infrastructure/             # Infrastructure layer
│       │   │   │   └── Repositories/           # EF Core implementations
│       │   │   ├── GraphQL/                    # GraphQL resolvers
│       │   │   ├── Data/                       # EF Core configurations
│       │   │   └── Models/                     # DTOs (UserDto, etc.)
│       │   └── Family/                         # Family bounded context
│       │       └── [same DDD structure]
│       ├── Common/                             # Shared infrastructure
│       │   ├── Domain/                         # Base domain classes
│       │   │   ├── AggregateRoot.cs
│       │   │   ├── IDomainEvent.cs
│       │   │   ├── DomainException.cs
│       │   │   └── ValueObjects/               # Shared value objects (Email)
│       │   ├── Application/                    # Application abstractions
│       │   │   ├── ICommand.cs, IQuery.cs
│       │   │   ├── ICommandBus.cs, IQueryBus.cs
│       │   ├── Infrastructure/                 # Infrastructure implementations
│       │   │   ├── Messaging/                  # Wolverine adapters
│       │   │   └── Database/                   # AppDbContext
│       │   ├── Authentication/                 # JWT setup
│       │   └── Middleware/                     # RLS, CORS, etc.
│       ├── Migrations/                         # EF Core migrations
│       └── Program.cs                          # Startup configuration
├── tests/
│   ├── FamilyHub.UnitTests/                    # Unit tests (aggregates, domain logic)
│   └── FamilyHub.IntegrationTests/             # Integration tests (handlers, database)
└── src/frontend/family-hub-web/                # Angular frontend
```

---

## DDD Implementation Patterns

### 1. Single API Project with DDD Layers

**Architecture**: Single `FamilyHub.Api` project with DDD layers as folders (not separate assemblies)

**Benefits**:

- ✅ DDD separation without multi-project complexity
- ✅ Faster compilation (single assembly)
- ✅ Simpler navigation
- ✅ Logical boundaries without physical overhead
- ✅ Easy to extract to microservices later (Strangler Fig pattern)

### 2. Feature-Based Modules with Clean Architecture

**Pattern**: Each feature has Domain, Application, Infrastructure layers

```
Features/Auth/
├── Domain/              # Core business logic (no dependencies)
│   ├── Entities/        # User aggregate root
│   ├── ValueObjects/    # UserId, UserName, ExternalUserId
│   ├── Events/          # UserRegisteredEvent, etc.
│   └── Repositories/    # IUserRepository interface
├── Application/         # Use cases (depends on Domain)
│   ├── Commands/        # RegisterUserCommand
│   ├── Queries/         # GetUserByIdQuery
│   ├── Handlers/        # Command/Query handlers
│   ├── Validators/      # FluentValidation validators
│   ├── EventHandlers/   # Domain event handlers
│   └── Mappers/         # UserMapper (aggregate → DTO)
├── Infrastructure/      # External concerns (depends on Application)
│   └── Repositories/    # UserRepository (EF Core)
├── GraphQL/            # GraphQL API layer
│   ├── AuthQueries.cs
│   └── AuthMutations.cs
├── Data/               # EF Core configurations
│   └── UserConfiguration.cs
└── Models/             # DTOs for GraphQL
    └── UserDto.cs
```

**Benefits**:

- ✅ Clear dependency flow (Domain ← Application ← Infrastructure ← GraphQL)
- ✅ Testable domain logic (no infrastructure dependencies)
- ✅ All related code in one feature folder
- ✅ Easy to understand bounded context boundaries

### 3. Vogen Value Objects

**Implementation**: Strongly-typed value objects with compile-time validation

```csharp
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email is required");
        if (!value.Contains('@'))
            return Validation.Invalid("Invalid email format");
        return Validation.Ok;
    }
}

// Usage
var email = Email.From("user@example.com");  // ✅ Valid
var invalid = Email.From("not-an-email");     // ❌ Throws VogenValidationException
```

**Benefits**:

- ✅ Type safety (can't assign UserId to FamilyId)
- ✅ Compile-time validation (invalid states impossible)
- ✅ EF Core converters auto-generated
- ✅ Self-documenting code

### 4. CQRS with Wolverine

**Pattern**: Separate read (queries) from write (commands) using Wolverine message bus

```csharp
// Command (write operation)
public sealed record CreateFamilyCommand(FamilyName Name, UserId OwnerId)
    : ICommand<CreateFamilyResult>;

// Query (read operation)
public sealed record GetMyFamilyQuery(ExternalUserId ExternalUserId)
    : IQuery<FamilyDto?>;

// Handler (auto-discovered by Wolverine)
public static class CreateFamilyCommandHandler
{
    public static async Task<CreateFamilyResult> Handle(
        CreateFamilyCommand command,
        IFamilyRepository repository,
        CancellationToken ct)
    {
        var family = Family.Create(command.Name, command.OwnerId);
        await repository.AddAsync(family, ct);
        await repository.SaveChangesAsync(ct);
        return new CreateFamilyResult(family.Id);
    }
}
```

**Benefits**:

- ✅ Clear intent (command vs query)
- ✅ Optimizable independently (read vs write paths)
- ✅ Convention-based handlers (no interface boilerplate)
- ✅ Wolverine combines mediator + message bus (future-proof)

### 5. Domain Events for Event Chains

**Pattern**: Aggregates raise events, handlers react to enable automation

```csharp
// Aggregate raises event
public static Family Create(FamilyName name, UserId ownerId)
{
    var family = new Family { /* ... */ };
    family.RaiseDomainEvent(new FamilyCreatedEvent(/* ... */));
    return family;
}

// Handler reacts (in-process via Wolverine)
public static class FamilyCreatedEventHandler
{
    public static Task Handle(FamilyCreatedEvent @event, ILogger logger)
    {
        logger.LogInformation("Family created: {FamilyId}", @event.FamilyId);
        // TODO: Create default calendar (event chain)
        // TODO: Create shared shopping list (event chain)
        return Task.CompletedTask;
    }
}
```

**Benefits**:

- ✅ Decouples modules (Family doesn't depend on Calendar)
- ✅ Enables event chain automation (strategic differentiator)
- ✅ Extensible (add workflows without changing aggregates)
- ✅ Observable (all events logged)

### 6. DDD Patterns In Use

**Using**:

- ✅ **Vogen value objects** - Type safety and validation
- ✅ **Wolverine CQRS** - Command/query separation
- ✅ **FluentValidation** - Command validation pipeline
- ✅ **DDD aggregates** - User, Family with business logic
- ✅ **Domain events** - Event-driven automation
- ✅ **Repository pattern** - Data access abstraction
- ✅ **Input→Command pattern** - GraphQL Input (primitives) → Command (value objects)

---

## Technology Stack

### Backend (DDD Implementation)

- **.NET Core 10** - Runtime platform
- **Hot Chocolate GraphQL 15.1.12** - GraphQL server with Input→Command pattern
- **Entity Framework Core 10.0.2** - ORM with Vogen value converters
- **PostgreSQL 16** - Database with Row-Level Security (RLS)
- **JWT Bearer authentication** - OAuth 2.0 token validation
- **Keycloak 23.0.4** - OAuth 2.0 / OIDC provider
- **WolverineFx 5.11.0** - CQRS message bus (MIT license, replaces MediatR)
- **Vogen 8.0.4** - Value object source generator
- **FluentValidation 11.11.0** - Command validation

### Frontend

- Angular 21 (standalone components)
- Apollo Client (GraphQL)
- Tailwind CSS 3.x
- TypeScript 5.x

### Infrastructure

- Docker Compose (local development)
- PostgreSQL with Row-Level Security (multi-tenant isolation)
- Keycloak 23.0.4 (authentication provider)

---

## OAuth 2.0 Flow (Keycloak)

### Authentication Flow

```
1. User clicks "Sign in" → Frontend redirects to Keycloak
2. User authenticates → Keycloak redirects back with auth code
3. Frontend exchanges code for tokens (PKCE) → Gets JWT
4. Frontend stores JWT in localStorage
5. Frontend sends GraphQL requests with Authorization header
6. Backend validates JWT → Extracts sub claim → Looks up user in database
7. Backend sets RLS variables from database (user.Id, user.FamilyId)
8. PostgreSQL enforces RLS policies → Returns only user's data
```

### JWT Claims (Standard OIDC Only)

**Keycloak provides only standard OIDC claims**:

- `sub` - Keycloak user ID (maps to User.ExternalUserId in database)
- `email` - User's email address
- `name` - User's display name
- `email_verified` - Email verification status (boolean)
- `exp` - Token expiration timestamp
- `iat` - Token issued at timestamp
- `iss` - Issuer (Keycloak realm URL)
- `aud` - Audience (familyhub-web)

**Family context comes from PostgreSQL**:

- `user.FamilyId` - Stored in database, queried via GraphQL
- Roles - Managed in database or Keycloak realm roles (not custom attributes)
- No custom JWT claims needed

---

## Database Multi-Tenancy (RLS)

### PostgreSQL Row-Level Security

**Two schemas**:

- `auth` - Users table
- `family` - Families table

**RLS Policies enforce**:

- Users can only see their own data
- Users can only see data from their family
- Enforced at database level (defense in depth)

**Implementation**:

1. JWT validated → Claims extracted
2. PostgresRlsMiddleware sets session variables
3. All queries automatically filtered by RLS policies

```sql
-- Set session variable from JWT
SELECT set_config('app.current_user_id', '{userId}', false);

-- RLS policy uses session variable
CREATE POLICY user_self_policy ON auth.users
    USING ("Id"::text = current_setting('app.current_user_id', true));
```

---

## Adding New Features (DDD Workflow)

### Step 1: Create Feature Folder Structure

```bash
mkdir -p src/FamilyHub.Api/Features/NewFeature/{Domain/{Entities,ValueObjects,Events,Repositories},Application/{Commands,Queries,Handlers,Validators,EventHandlers,Mappers},Infrastructure/Repositories,GraphQL,Data,Models}
```

### Step 2: Create Value Objects

```csharp
// Features/NewFeature/Domain/ValueObjects/NewEntityId.cs
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct NewEntityId
{
    public static NewEntityId New() => From(Guid.NewGuid());
}

// Features/NewFeature/Domain/ValueObjects/NewEntityName.cs
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct NewEntityName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Name is required");
        return Validation.Ok;
    }
}
```

### Step 3: Create Aggregate Root

```csharp
// Features/NewFeature/Domain/Entities/NewEntity.cs
public sealed class NewEntity : AggregateRoot<NewEntityId>
{
    public NewEntityName Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static NewEntity Create(NewEntityName name)
    {
        var entity = new NewEntity
        {
            Id = NewEntityId.New(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        entity.RaiseDomainEvent(new NewEntityCreatedEvent(entity.Id, entity.Name, DateTime.UtcNow));
        return entity;
    }
}
```

### Step 4: Create Domain Events

```csharp
// Features/NewFeature/Domain/Events/NewEntityCreatedEvent.cs
public sealed record NewEntityCreatedEvent(
    NewEntityId EntityId,
    NewEntityName Name,
    DateTime CreatedAt
) : DomainEvent;
```

### Step 5: Create Repository

```csharp
// Features/NewFeature/Domain/Repositories/INewEntityRepository.cs
public interface INewEntityRepository
{
    Task<NewEntity?> GetByIdAsync(NewEntityId id, CancellationToken ct = default);
    Task AddAsync(NewEntity entity, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// Features/NewFeature/Infrastructure/Repositories/NewEntityRepository.cs
public sealed class NewEntityRepository : INewEntityRepository
{
    private readonly AppDbContext _context;
    // ... implementation
}
```

### Step 6: Create Commands & Handlers

```csharp
// Features/NewFeature/Application/Commands/CreateNewEntityCommand.cs
public sealed record CreateNewEntityCommand(NewEntityName Name)
    : ICommand<CreateNewEntityResult>;

// Features/NewFeature/Application/Handlers/CreateNewEntityCommandHandler.cs
public static class CreateNewEntityCommandHandler
{
    public static async Task<CreateNewEntityResult> Handle(
        CreateNewEntityCommand command,
        INewEntityRepository repository,
        CancellationToken ct)
    {
        var entity = NewEntity.Create(command.Name);
        await repository.AddAsync(entity, ct);
        await repository.SaveChangesAsync(ct);
        return new CreateNewEntityResult(entity.Id);
    }
}
```

### Step 7: Create Validator

```csharp
// Features/NewFeature/Application/Validators/CreateNewEntityCommandValidator.cs
public sealed class CreateNewEntityCommandValidator : AbstractValidator<CreateNewEntityCommand>
{
    public CreateNewEntityCommandValidator()
    {
        RuleFor(x => x.Name).NotNull().WithMessage("Name is required");
    }
}
```

### Step 8: Create Event Handler

```csharp
// Features/NewFeature/Application/EventHandlers/NewEntityCreatedEventHandler.cs
public static class NewEntityCreatedEventHandler
{
    public static Task Handle(NewEntityCreatedEvent @event, ILogger logger)
    {
        logger.LogInformation("NewEntity created: {EntityId}", @event.EntityId);
        // TODO: Trigger event chains
        return Task.CompletedTask;
    }
}
```

### Step 9: Create EF Core Configuration

```csharp
// Features/NewFeature/Data/NewEntityConfiguration.cs
public class NewEntityConfiguration : IEntityTypeConfiguration<NewEntity>
{
    public void Configure(EntityTypeBuilder<NewEntity> builder)
    {
        builder.ToTable("new_entities", "newfeature");

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => NewEntityId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .HasConversion(name => name.Value, value => NewEntityName.From(value))
            .IsRequired();
    }
}
```

### Step 10: Update AppDbContext

```csharp
// Common/Database/AppDbContext.cs
public DbSet<NewEntity> NewEntities { get; set; }
```

### Step 11: Create GraphQL Layer

```csharp
// Features/NewFeature/GraphQL/NewFeatureMutations.cs
public class NewFeatureMutations
{
    [Authorize]
    public async Task<NewEntityDto> CreateNewEntity(
        CreateNewEntityInput input,  // GraphQL Input (primitives)
        [Service] ICommandBus commandBus,
        [Service] INewEntityRepository repository,
        CancellationToken ct)
    {
        // Input → Command pattern (primitives → value objects)
        var name = NewEntityName.From(input.Name);
        var command = new CreateNewEntityCommand(name);

        // Send command via Wolverine
        var result = await commandBus.SendAsync<CreateNewEntityResult>(command, ct);

        // Query and return DTO
        var entity = await repository.GetByIdAsync(result.EntityId, ct);
        return NewEntityMapper.ToDto(entity!);
    }
}
```

### Step 12: Register in Program.cs

```csharp
// Register repository
builder.Services.AddScoped<INewEntityRepository, NewEntityRepository>();

// Register GraphQL (validators and handlers auto-discovered by Wolverine)
builder.Services
    .AddGraphQLServer()
    .AddTypeExtension<NewFeatureMutations>();
```

**Note:** FluentValidation validators and Wolverine handlers are auto-discovered via assembly scanning.

---

## Migration Path

### When to Extract to Modules/Services

**Phase 2-3** (Optional):

- If a feature exceeds 15 files
- If a feature has distinct deployment needs
- If performance requires service separation

**Phase 5+** (Microservices):

- Extract to separate services with own databases
- Use message broker (RabbitMQ) for inter-service communication
- Deploy to Kubernetes

**Migration Strategy**: Strangler Fig Pattern

1. Create new service
2. Dual-write to both
3. Migrate reads
4. Remove from monolith

---

## Testing Strategy

### Unit Tests

Test services and business logic in isolation using in-memory database:

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;

var context = new AppDbContext(options);
var service = new AuthService(context);
```

### Integration Tests

Test full HTTP pipeline using WebApplicationFactory:

```csharp
public class GraphQLApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }
}
```

---

## Comparison: Simplified vs. DDD Implementation

| Aspect | Simplified (Commit #112) | DDD Implementation (Current) |
|--------|--------------------------|------------------------------|
| **Projects** | 3 (API, 2 test) | 3 (API, 2 test) - Same |
| **Organization** | Feature folders (flat) | Feature folders with DDD layers |
| **Entities** | Anemic POCOs | Rich aggregates with domain logic |
| **DbContext** | Single AppDbContext | Single AppDbContext - Same |
| **Value Objects** | Primitives (Guid, string) | Vogen value objects (type-safe) |
| **Commands** | Direct service calls | Wolverine CQRS with handlers |
| **Validation** | Inline checks | FluentValidation pipeline |
| **Domain Events** | None | Full event infrastructure |
| **Event Chains** | ❌ Not possible | ✅ Enabled via domain events |
| **Repositories** | Direct DbContext | Repository interfaces |
| **Messaging** | None | Wolverine (in-process, RabbitMQ-ready) |
| **RLS** | Middleware only | Database policies enforced |
| **Testability** | Low (services depend on DbContext) | High (aggregates + mocked repositories) |
| **LOC** | ~5,000 | ~8,000 |

---

## Implementation Summary (Issue #113)

**Phases Completed:**

1. ✅ Foundation Setup - Wolverine, Vogen, base classes (8-10 hrs)
2. ✅ Value Objects - 6 value objects with validation (10-12 hrs)
3. ✅ Auth Module DDD - User aggregate, commands, handlers (12-16 hrs)
4. ✅ Family Module DDD - Family aggregate, commands, handlers (12-16 hrs)
5. ✅ Query Layer CQRS - Query objects and handlers (8-10 hrs)
6. ✅ Domain Event Handlers - 6 event handlers (6-8 hrs)
7. ✅ RLS Enforcement - PostgreSQL policies (4-6 hrs)
8. ✅ Testing & Documentation - Unit tests, docs (8-10 hrs)

**Total Effort:** 60-80 hours (completed with Claude Code assistance)

---

## References

- **ADR-001**: Modular Monolith First (DDD within single project)
- **ADR-002**: OAuth with Keycloak
- **ADR-003**: GraphQL Input→Command Pattern (implemented)
- **BACKEND_DEVELOPMENT.md**: DDD development guide
- **PATTERNS.md**: DDD patterns reference
- **Wolverine**: https://wolverinefx.net (CQRS message bus)
- **Vogen**: https://github.com/SteveDunn/Vogen (value object generator)

---

**Last Updated**: 2026-02-02
**Author**: Claude Sonnet 4.5 (Issue #113 DDD Restoration)
**Status**: Active - DDD architecture fully implemented
