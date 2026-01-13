# Development Workflows

**Purpose:** Detailed patterns for common development tasks. Load this document when implementing features that require specific workflow knowledge.

**When to reference:** Database migrations, value objects, testing, GraphQL integration, E2E tests.

---

## Database Migrations with EF Core

**CRITICAL:** Use EF Core Code-First migrations for ALL schema changes (never custom SQL scripts).

### Pattern

One DbContext per module (Auth, Calendar, etc.), each targeting its own PostgreSQL schema. Fluent API configurations in `IEntityTypeConfiguration<T>` classes, PostgreSQL-specific features (RLS, triggers) via `migrationBuilder.Sql()`.

### Commands

```bash
# Create migration
dotnet ef migrations add <Name> --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api

# Apply migration (development)
dotnet ef database update --context AuthDbContext

# Production (in Program.cs)
await context.Database.MigrateAsync();
```

### Vogen Integration

```csharp
// In IEntityTypeConfiguration<User>
builder.Property(u => u.Id)
    .HasConversion(new UserId.EfCoreValueConverter())
    .IsRequired();
```

### Reference

Original SQL design scripts in `/database/docs/reference/sql-design/` (informational only, NOT executed).

---

## Value Objects with Vogen

**CRITICAL:** Use Vogen for ALL value objects (never manual base classes).

### Pattern

```csharp
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct UserId
{
    // Vogen source generator auto-generates:
    // - Equality operators
    // - EF Core converter
    // - JSON serialization
    // - Validation
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email cannot be empty");

        if (!value.Contains('@'))
            return Validation.Invalid("Invalid email format");

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
        => input?.Trim().ToLowerInvariant() ?? string.Empty;
}
```

### Creation

```csharp
// New GUID
UserId userId = UserId.New();

// With validation
Email email = Email.From("user@example.com"); // Throws if invalid
Email.TryFrom("invalid", out var result);     // Safe creation

// In tests (manual creation)
var testEmail = Email.From("test@example.com");
```

### EF Core Configuration

```csharp
builder.Property(u => u.Email)
    .HasConversion(new Email.EfCoreValueConverter())
    .HasMaxLength(255);
```

### Examples

See `/src/api/FamilyHub.SharedKernel/Domain/ValueObjects/` for Email, UserId, FamilyId patterns.

---

## DbContext Usage Patterns

**CRITICAL:** One DbContext per module, registered with pooled factory for DataLoader compatibility.

### Pattern

Each module has its own DbContext targeting a dedicated PostgreSQL schema. This enforces bounded context boundaries at the database level and enables independent migrations per module.

Key principles:

- **One DbContext per module** (AuthDbContext, FamilyDbContext)
- **Schema isolation** via `modelBuilder.HasDefaultSchema()`
- **Auto-discovery** via `ApplyConfigurationsFromAssembly()`
- **Pooled factory** for DataLoader compatibility

### DbContext Structure

```csharp
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    // Module entities as DbSet properties
    public DbSet<User> Users => Set<User>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set PostgreSQL schema for this module
        modelBuilder.HasDefaultSchema("auth");

        // Apply all configurations from this assembly (auto-discovery)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
```

### Module Registration

Register DbContext with pooled factory in module service registration:

```csharp
public static IServiceCollection AddAuthModule(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // CRITICAL: Use pooled factory for DataLoader compatibility
    services.AddPooledDbContextFactory<AuthDbContext>((sp, options) =>
    {
        var connectionString = configuration.GetConnectionString("FamilyHubDb");
        options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Specify migrations assembly for integration tests
                npgsqlOptions.MigrationsAssembly(typeof(AuthDbContext).Assembly.GetName().Name);
            })
            .UseSnakeCaseNamingConvention()
            .AddTimestampInterceptor(sp);
    });

    // Register scoped DbContext for normal use (repositories, handlers)
    services.AddScoped(sp =>
    {
        var factory = sp.GetRequiredService<IDbContextFactory<AuthDbContext>>();
        return factory.CreateDbContext();
    });

    return services;
}
```

### IDbContextFactory for DataLoaders

**Why pooled factory?** DataLoaders have request-scoped lifetime but batch queries across multiple resolver calls. Using scoped DbContext causes concurrency issues. `IDbContextFactory` creates fresh DbContext instances per batch operation.

```csharp
// In DataLoader - use factory, NOT scoped DbContext
public sealed class UserBatchDataLoader : BatchDataLoader<UserId, User>
{
    private readonly IDbContextFactory<AuthDbContext> _dbContextFactory;

    protected override async Task<IReadOnlyDictionary<UserId, User>> LoadBatchAsync(
        IReadOnlyList<UserId> keys,
        CancellationToken cancellationToken)
    {
        // Create fresh DbContext for this batch
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Users
            .Where(u => keys.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);
    }
}
```

See [How to Create DataLoaders](#how-to-create-dataloaders) for complete DataLoader implementation guide.

### Cross-Module References

Modules reference entities from other modules using **value object IDs only** (no foreign key constraints across module boundaries):

```csharp
// In User entity (Auth module)
public FamilyId FamilyId { get; private set; }  // Reference to Family module

// NO FK constraint - modules are decoupled
// Cross-module data fetched via GraphQL resolvers + DataLoaders
```

This approach enables:

- Independent module deployment (Phase 5+ microservices)
- Module-specific schema migrations
- Clear bounded context boundaries

### Reference

- [ADR-001: Modular Monolith First](../architecture/ADR-001-MODULAR-MONOLITH-FIRST.md)
- [Database Guide](../../database/CLAUDE.md)

---

## GraphQL Input/Command Pattern

**CRITICAL:** Maintain separate GraphQL Input DTOs (primitive types) that map to MediatR Commands (Vogen value objects).

### Why

HotChocolate cannot natively deserialize Vogen value objects from JSON. Input → Command mapping provides explicit conversion point and framework compatibility.

### Pattern

```csharp
// GraphQL Input DTO (primitives)
public record CreateFamilyInput
{
    public string Name { get; init; } = string.Empty;
}

// MediatR Command (Vogen value objects)
public record CreateFamilyCommand(FamilyName Name) : IRequest<CreateFamilyPayload>;

// Mutation method
public async Task<CreateFamilyPayload> CreateFamilyAsync(
    CreateFamilyInput input,
    [Service] IMediator mediator)
{
    var command = new CreateFamilyCommand(
        FamilyName.From(input.Name) // Explicit conversion
    );

    return await mediator.Send(command);
}
```

### Decision Rationale

[ADR-003](../architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md) - Attempted command-as-input pattern, failed due to Vogen incompatibility.

---

## GraphQL Mapper Pattern

**CRITICAL:** Use convention-based mapper pattern for converting command results to GraphQL payload types (replaces old factory DI pattern).

### Why

The mapper pattern provides:

- **70% reduction in boilerplate** - No factory classes or DI registration
- **Zero code duplication** - Centralized mappers reused across mutations
- **Auto-mapping for simple cases** - Property name matching with intelligent type conversion
- **Manual override when needed** - ToGraphQLType() extensions for complex mappings
- **Better architecture** - Eliminates post-command repository calls in presentation layer

### Two Approaches

#### Auto-Mapping Convention (Default)

MutationHandler automatically maps command result properties to payload constructor parameters:

**Features:**

- Case-insensitive property name matching
- Automatic Vogen `.Value` unwrapping
- Automatic enum helper detection (e.g., `Role.AsRoleType()`)
- Supports parameterless, single-param, and multi-param (tuple) constructors

**When to use:**

- Simple property mappings
- Basic type conversions (Guid, string, DateTime, bool, int)
- Vogen value objects that need unwrapping
- Enum conversions with `.AsXxxType()` helper methods

**Example (no code needed):**

```csharp
// Command result
public record AcceptInvitationResult(FamilyId FamilyId, FamilyName FamilyName, UserRole Role);

// Payload constructor (auto-mapping matches properties by name)
public AcceptInvitationPayload(Guid familyId, string familyName, UserRoleType role)
{
    FamilyId = familyId;
    FamilyName = familyName;
    Role = role;
}

// No ToGraphQLType() needed! MutationHandler:
// 1. Matches FamilyId → familyId (case-insensitive)
// 2. Unwraps FamilyId.Value (Vogen)
// 3. Auto-detects Role.AsRoleType() extension for enum conversion
```

**Error Handling:**

If auto-mapping fails, you get a descriptive `AutoMappingException`:

```
Failed to auto-map AcceptInvitationResult to AcceptInvitationPayload:
Property 'InvalidField' not found in result type.
Consider adding a ToGraphQLType() extension method.
```

#### Manual Override (Complex Cases)

Add a `ToGraphQLType()` extension method when auto-mapping can't handle:

**When to use:**

- Nested object creation
- Calculated fields
- Multiple enum conversions with different helpers
- Complex transformations

**Example:**

```csharp
// Extension method in AuthResultExtensions.cs
public static class AuthResultExtensions
{
    // Complex nested object - requires manual mapping
    public static AuthenticationResult ToGraphQLType(this CompleteZitadelLoginResult result)
    {
        return new AuthenticationResult
        {
            User = new UserType  // Nested object creation
            {
                Id = result.UserId.Value,
                Email = result.Email.Value,
                EmailVerified = result.EmailVerified,
                FamilyId = result.FamilyId.Value,
                AuditInfo = result.AsAuditInfo()  // Custom mapping
            },
            AccessToken = result.AccessToken,
            RefreshToken = null,
            ExpiresAt = result.ExpiresAt
        };
    }

    // Calculated field - requires manual mapping
    public static CreatedFamilyDto ToGraphQLType(this CreateFamilyResult result)
    {
        return new CreatedFamilyDto
        {
            Id = result.FamilyId.Value,
            Name = result.Name.Value,
            OwnerId = result.OwnerId.Value,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.CreatedAt  // Calculated: same as CreatedAt for new families
        };
    }
}
```

**Precedence:** If both exist, manual ToGraphQLType() takes precedence over auto-mapping.

### Pattern

```csharp
// 1. MAPPERS - Centralized mapping logic (Presentation/GraphQL/Mappers/)
public static class UserMapper
{
    // Maps domain entity to GraphQL type
    public static UserType AsGraphQLType(User user)
    {
        return new UserType
        {
            Id = user.Id.Value,
            Email = user.Email.Value,
            EmailVerified = user.EmailVerified,
            FamilyId = user.FamilyId.Value,
            AuditInfo = MapperBase.AsAuditInfo(user.CreatedAt, user.UpdatedAt)
        };
    }
}

public static class InvitationMapper
{
    // Maps value object enums to GraphQL enums
    public static UserRoleType AsRoleType(UserRole role)
    {
        return role.Value.ToLowerInvariant() switch
        {
            "owner" => UserRoleType.OWNER,
            "admin" => UserRoleType.ADMIN,
            "member" => UserRoleType.MEMBER,
            _ => throw new InvalidOperationException($"Unknown role: {role.Value}")
        };
    }
}

// 2. EXTENSIONS - ToGraphQLType() for each command result (Presentation/GraphQL/Extensions/)
public static class AuthResultExtensions
{
    // Single object return (most common)
    public static AuthenticationResult ToGraphQLType(this CompleteZitadelLoginResult result)
    {
        return new AuthenticationResult
        {
            User = UserMapper.AsUserType(
                result.UserId,
                result.Email,
                result.EmailVerified,
                result.FamilyId,
                result.CreatedAt,
                result.UpdatedAt),
            AccessToken = result.AccessToken,
            ExpiresAt = result.ExpiresAt
        };
    }

    // Tuple return (multiple constructor parameters)
    public static (Guid FamilyId, string FamilyName, UserRoleType Role) ToGraphQLType(
        this AcceptInvitationResult result)
    {
        return (
            result.FamilyId.Value,
            result.FamilyName.Value,
            InvitationMapper.AsRoleType(result.Role)
        );
    }

    // Null return (parameterless constructor)
    public static object? ToGraphQLType(this Result result)
    {
        return null; // Signals MutationHandler to use parameterless constructor
    }
}

// 3. USAGE - MutationHandler automatically discovers and invokes extensions
public async Task<CompleteZitadelLoginPayload> CompleteZitadelLoginAsync(
    CompleteZitadelLoginInput input,
    [Service] IMutationHandler mutationHandler,
    [Service] IMediator mediator)
{
    return await mutationHandler.Handle<CompleteZitadelLoginResult, CompleteZitadelLoginPayload>(
        async () =>
        {
            var command = new CompleteZitadelLoginCommand(
                AuthorizationCode.From(input.Code),
                ZitadelCallbackUri.From(input.RedirectUri));

            var result = await mediator.Send(command);
            return result; // MutationHandler calls result.ToGraphQLType() via reflection
        });
}
```

### Three ToGraphQLType() Patterns

The MutationHandler supports three constructor patterns:

#### 1. Single Object Return (Most Common)

```csharp
// Extension returns single object
public static CreatedFamilyDto ToGraphQLType(this CreateFamilyResult result)
{
    return new CreatedFamilyDto
    {
        Id = result.FamilyId.Value,
        Name = result.Name.Value,
        OwnerId = result.OwnerId.Value,
        CreatedAt = result.CreatedAt,
        UpdatedAt = result.CreatedAt
    };
}

// Payload constructor (single parameter)
public CreateFamilyPayload(CreatedFamilyDto family)
{
    Family = family;
}
```

#### 2. Tuple Return (Multiple Parameters)

```csharp
// Extension returns tuple
public static (Guid InvitationId, UserRoleType Role) ToGraphQLType(
    this UpdateInvitationRoleResult result)
{
    return (
        result.InvitationId.Value,
        InvitationMapper.AsRoleType(result.Role)
    );
}

// Payload constructor (multiple parameters matching tuple)
public UpdateInvitationRolePayload(Guid invitationId, UserRoleType role)
{
    InvitationId = invitationId;
    Role = role;
}
```

#### 3. Null Return (Parameterless Constructor)

```csharp
// Extension returns null
public static object? ToGraphQLType(this Result result)
{
    return null; // Signals parameterless constructor
}

// Payload constructor (parameterless)
public CancelInvitationPayload()
{
    IsSuccess = true;
}
```

### Auto-Mapping Algorithm

MutationHandler follows this decision tree:

1. **Check for manual ToGraphQLType()** - If found, use it (manual override takes precedence)
2. **Analyze payload constructor:**
   - **Parameterless** → Return null (signals parameterless constructor)
   - **Single parameter** → Find matching result property, extract value
   - **Multiple parameters** → Build tuple from matched properties
3. **Property matching:** Case-insensitive name match (e.g., `FamilyId` → `familyId`)
4. **Value extraction:**
   - Direct match (primitives: Guid, string, DateTime, bool, int)
   - Vogen unwrapping (detect `.Value` property)
   - Enum helper detection (auto-find `.AsXxxType()` extension methods)
5. **Error handling:** Throw `AutoMappingException` with descriptive message if any step fails

**Performance:** Reflection results cached in `ConcurrentDictionary` (<5ms overhead per mutation)

**Limitations:**

- Supports up to 5 constructor parameters (C# tuple limitation)
- Cannot auto-map nested object creation
- Cannot handle calculated fields or complex transformations

### Directory Structure

```
Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/
├── Mappers/
│   ├── UserMapper.cs          # Domain entity → GraphQL type
│   ├── InvitationMapper.cs    # Enum mappings, shared logic
│   └── FamilyMapper.cs        # Family-specific mappings
├── Extensions/
│   └── AuthResultExtensions.cs # ToGraphQLType() for all Auth command results
├── Payloads/
│   ├── CompleteZitadelLoginPayload.cs
│   ├── CreateFamilyPayload.cs
│   └── AcceptInvitationPayload.cs
└── Mutations/
    └── AuthMutations.cs       # GraphQL mutation methods
```

### Naming Conventions

- **Mappers**: `AsGraphQLType()` or `As{TargetType}()` (e.g., `AsRoleType()`)
- **Extensions**: `ToGraphQLType()` (MUST be this exact name for source generator)
- **Location**: `{Module}.Presentation.GraphQL.Mappers` and `.Extensions` namespaces

### Error Handling

MutationHandler ONLY handles errors - mappers handle success cases:

```csharp
// GOOD - Mapper only handles success case
public static UserType AsGraphQLType(User user)
{
    return new UserType { /* ... */ };
}

// BAD - Don't handle errors in mappers
public static UserType? AsGraphQLType(User? user)
{
    if (user == null)
        return null; // MutationHandler already handles this
    // ...
}
```

### Migration from Old Pattern

Old pattern (deprecated):

```csharp
// Factory class with DI (DELETED)
public class CreateFamilyPayloadFactory(IFamilyRepository repository)
    : IPayloadFactory<CreateFamilyResult, CreateFamilyPayload>
{
    public CreateFamilyPayload Success(CreateFamilyResult result)
    {
        // Anti-pattern: Repository call in presentation layer
        var family = repository.GetByIdAsync(result.FamilyId).GetAwaiter().GetResult();
        return new CreateFamilyPayload(family);
    }
}

// DI registration (DELETED)
services.AddScoped<IPayloadFactory<CreateFamilyResult, CreateFamilyPayload>,
    CreateFamilyPayloadFactory>();
```

New pattern:

```csharp
// Static extension (NO DI)
public static CreatedFamilyDto ToGraphQLType(this CreateFamilyResult result)
{
    return new CreatedFamilyDto
    {
        Id = result.FamilyId.Value,
        Name = result.Name.Value,
        OwnerId = result.OwnerId.Value,
        CreatedAt = result.CreatedAt,
        UpdatedAt = result.CreatedAt // Use data already in result
    };
}

// NO DI registration needed - convention-based discovery
```

### Common Utilities

`MapperBase` provides shared mapping logic:

```csharp
public static class MapperBase
{
    public static AuditInfoType AsAuditInfo(DateTime createdAt, DateTime updatedAt)
    {
        return new AuditInfoType
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}
```

Location: `FamilyHub.Infrastructure/GraphQL/MapperBase.cs`

### Testing

Mappers are pure functions - easy to unit test:

```csharp
[Fact]
public void AsGraphQLType_ValidUser_MapsCorrectly()
{
    // Arrange
    var user = new User
    {
        Id = UserId.New(),
        Email = Email.From("test@example.com"),
        EmailVerified = true,
        FamilyId = FamilyId.New(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    // Act
    var result = UserMapper.AsGraphQLType(user);

    // Assert
    result.Id.Should().Be(user.Id.Value);
    result.Email.Should().Be(user.Email.Value);
    result.EmailVerified.Should().BeTrue();
}
```

### Reference

- MutationHandler: `src/api/FamilyHub.SharedKernel/Presentation/GraphQL/MutationHandler.cs`
- AutoMappingException: `src/api/FamilyHub.SharedKernel/Presentation/GraphQL/AutoMappingException.cs`
- Example implementation: `src/api/Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/`
- Auth migrations: 3/6 Auth mutations use auto-mapping, 3/6 use manual override

---

## How to Create DataLoaders

**CRITICAL:** Use DataLoaders to prevent N+1 query problems in GraphQL resolvers.

### Overview

DataLoaders batch multiple concurrent data requests into single database queries. Without DataLoaders, GraphQL nested resolvers cause N+1 performance problems (e.g., loading 100 users requires 101 queries instead of 2).

Family Hub uses Hot Chocolate's GreenDonut library for DataLoader implementation. Performance impact (from ADR-011):

| Scenario | Without DataLoaders | With DataLoaders | Improvement |
|----------|---------------------|------------------|-------------|
| 10 families | 11 queries | 2 queries | 5.5x |
| 100 families | 101 queries | 2 queries | 50.5x |
| 1,000 families | 1,001 queries | 2 queries | 500.5x |

### Two DataLoader Types

| Type | Use Case | Return Type |
|------|----------|-------------|
| `BatchDataLoader<TKey, TValue>` | 1:1 lookups (user by ID) | `IReadOnlyDictionary<TKey, TValue>` |
| `GroupedDataLoader<TKey, TValue>` | 1:N lookups (users by family) | `ILookup<TKey, TValue>` |

### Creating a BatchDataLoader (1:1)

Use BatchDataLoader when each key returns exactly one entity:

```csharp
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.DataLoaders;

/// <summary>
/// Batches user lookups by ID into a single database query.
/// Uses IDbContextFactory for proper DbContext pooling with DataLoader lifetime.
/// </summary>
public sealed class UserBatchDataLoader : BatchDataLoader<UserId, User>
{
    private readonly IDbContextFactory<AuthDbContext> _dbContextFactory;

    public UserBatchDataLoader(
        IDbContextFactory<AuthDbContext> dbContextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options)
        : base(batchScheduler, options)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task<IReadOnlyDictionary<UserId, User>> LoadBatchAsync(
        IReadOnlyList<UserId> keys,
        CancellationToken cancellationToken)
    {
        // Create fresh DbContext for this batch (critical for DataLoader lifetime)
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Single query with WHERE id IN (...) for all requested users
        // The Vogen EfCoreValueConverter handles UserId <-> Guid conversion
        return await dbContext.Users
            .Where(u => keys.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);
    }
}
```

### Creating a GroupedDataLoader (1:N)

Use GroupedDataLoader when each key returns multiple entities:

```csharp
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.DataLoaders;

/// <summary>
/// Groups users by FamilyId for efficient batch loading.
/// Used for resolving Family.Members (1:N relationship).
/// </summary>
public sealed class UsersByFamilyGroupedDataLoader : GroupedDataLoader<FamilyId, User>
{
    private readonly IDbContextFactory<AuthDbContext> _dbContextFactory;

    public UsersByFamilyGroupedDataLoader(
        IDbContextFactory<AuthDbContext> dbContextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options)
        : base(batchScheduler, options)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task<ILookup<FamilyId, User>> LoadGroupedBatchAsync(
        IReadOnlyList<FamilyId> keys,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Single query with WHERE family_id IN (...) for all requested families
        var users = await dbContext.Users
            .Where(u => keys.Contains(u.FamilyId))
            .ToListAsync(cancellationToken);

        // Group users by their FamilyId for 1:N relationship resolution
        return users.ToLookup(u => u.FamilyId);
    }
}
```

### Registration

**Order is critical:** Register DbContext factories BEFORE DataLoaders.

**Step 1: Module Registration** (in module service registration file):

```csharp
// DbContext factory with pooling (already done in AddAuthModule)
services.AddPooledDbContextFactory<AuthDbContext>((sp, options) => { ... });
```

**Step 2: GraphQL Registration** (in Program.cs):

```csharp
var graphqlBuilder = services
    .AddGraphQLServer()
    .AddQueryType()
    .AddMutationType();

// Module GraphQL types (includes DbContext factory registration for GraphQL)
graphqlBuilder
    .AddAuthModuleGraphQlTypes()
    .AddFamilyModuleGraphQlTypes();

// DataLoader registration AFTER DbContext factories
graphqlBuilder
    .AddDataLoader<UserBatchDataLoader>()
    .AddDataLoader<UsersByFamilyGroupedDataLoader>()
    .AddDataLoader<FamilyBatchDataLoader>()
    .AddDataLoader<InvitationsByFamilyGroupedDataLoader>();
```

### Using in Resolvers

DataLoaders are auto-injected by Hot Chocolate (no `[Service]` attribute needed):

```csharp
[ExtendObjectType(typeof(FamilyType))]
public sealed class FamilyTypeExtensions
{
    [GraphQLDescription("The owner of this family")]
    public async Task<UserType?> GetOwner(
        [Parent] FamilyAggregate family,
        UserBatchDataLoader userDataLoader,  // Auto-injected
        CancellationToken cancellationToken)
    {
        var owner = await userDataLoader.LoadAsync(family.OwnerId, cancellationToken);
        return owner == null ? null : UserMapper.AsGraphQLType(owner);
    }

    [GraphQLDescription("All members of this family")]
    public async Task<IEnumerable<UserType>> GetMembers(
        [Parent] FamilyAggregate family,
        UsersByFamilyGroupedDataLoader membersDataLoader,  // Auto-injected
        CancellationToken cancellationToken)
    {
        var members = await membersDataLoader.LoadAsync(family.Id, cancellationToken);
        return members.Select(UserMapper.AsGraphQLType);
    }
}
```

### Testing DataLoaders

**Unit Test Pattern** (verify batching):

```csharp
[Fact]
public async Task LoadBatchAsync_WithMultipleKeys_ShouldQueryDatabaseOnce()
{
    // Arrange
    var factory = _fixture.CreateMockFactoryWithCallTracking(out var callCount);
    var sut = new UserBatchDataLoader(factory, _batchScheduler, _options);

    // Act - Load multiple keys (should be batched)
    var task1 = sut.LoadAsync(user1.Id, CancellationToken.None);
    var task2 = sut.LoadAsync(user2.Id, CancellationToken.None);
    await Task.WhenAll(task1, task2);

    // Assert - Should be batched into single query
    callCount[0].Should().Be(1, "DataLoader should batch all keys into a single query");
}
```

**Integration Test:** See `DataLoaderQueryCountTests.cs` for real database query counting.

### Naming Conventions

| Pattern | Example | Use Case |
|---------|---------|----------|
| `{Entity}BatchDataLoader` | `UserBatchDataLoader` | 1:1 lookups by ID |
| `{Entities}By{Key}GroupedDataLoader` | `UsersByFamilyGroupedDataLoader` | 1:N lookups |

### Checklist

When creating a new DataLoader:

- [ ] Choose correct type: `BatchDataLoader<K,V>` (1:1) or `GroupedDataLoader<K,V>` (1:N)
- [ ] Inject `IDbContextFactory<T>` (NOT scoped DbContext)
- [ ] Use `await using` for DbContext disposal
- [ ] Use `WHERE IN` clause for batching
- [ ] Add XML documentation explaining purpose
- [ ] Register in Program.cs (after DbContext factories)
- [ ] Add unit tests with call tracking
- [ ] Add resolver that uses the DataLoader

### Reference

- [ADR-011: DataLoader Pattern](../architecture/ADR-011-DATALOADER-PATTERN.md)
- [DataLoader Performance Benchmarks](#dataloader-performance-benchmarks)
- Location: `src/api/Modules/*/Presentation/GraphQL/DataLoaders/`
- Tests: `src/api/tests/FamilyHub.Tests.Unit/DataLoaders/`

---

## Testing Patterns

### FluentAssertions

**CRITICAL:** Use FluentAssertions for ALL assertions (never xUnit `Assert.*`).

```csharp
// Basic assertions
actual.Should().Be(expected);
result.Should().NotBeNull();
collection.Should().HaveCount(3);

// Async assertions
await act.Should().ThrowAsync<InvalidOperationException>();
await task.Should().CompleteWithinAsync(TimeSpan.FromSeconds(5));

// Object assertions
user.Should().BeEquivalentTo(expected, options => options
    .Excluding(u => u.Id)
    .Excluding(u => u.CreatedAt));
```

Docs: <https://fluentassertions.com/>

### AutoFixture with NSubstitute

**CRITICAL:** Use `[Theory, AutoNSubstituteData]` for ALL tests with dependencies.

```csharp
[Theory, AutoNSubstituteData]
public async Task CreateFamily_Success(
    // Dependencies auto-injected by AutoFixture
    IFamilyRepository repository,
    IMediator mediator,
    CreateFamilyCommand command)
{
    // Arrange - configure only what matters
    repository.ExistsByNameAsync(Arg.Any<FamilyName>())
        .Returns(false);

    // Act
    var handler = new CreateFamilyCommandHandler(repository, mediator);
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    await repository.Received(1).AddAsync(Arg.Any<Family>());
}
```

**Vogen Policy:** Always create Vogen value objects manually in tests (improves clarity):

```csharp
// GOOD
var familyName = FamilyName.From("Test Family");
var userId = UserId.New();

// BAD (don't let AutoFixture generate Vogen types)
// AutoFixture can't properly generate valid Vogen instances
```

Custom attribute location: `/src/api/tests/FamilyHub.Tests.Unit/AutoNSubstituteDataAttribute.cs`

---

## E2E Testing with Playwright

**CRITICAL:** Use Playwright for ALL E2E tests (migrated from Cypress January 2026).

### Test Structure

```
e2e/
├── fixtures/          # Reusable test fixtures (auth, graphql, rabbitmq)
├── support/           # Helper utilities (constants, vogen-mirrors, api-helpers)
├── tests/             # Test files (.spec.ts)
├── global-setup.ts    # Testcontainers lifecycle
└── global-teardown.ts # Cleanup
```

### Key Patterns

#### 1. Fixtures (Dependency Injection)

```typescript
test("should create family", async ({
  authenticatedPage,
  interceptGraphQL,
}) => {
  await interceptGraphQL("GetCurrentFamily", { data: { family: null } });
  await authenticatedPage.goto("/family/create");
  // Test uses OAuth tokens automatically
});
```

#### 2. Vogen TypeScript Mirrors

```typescript
// Mirror C# Vogen validation in TypeScript
const familyName = FamilyName.from("Smith Family"); // Throws if invalid
const userId = UserId.new(); // Generates new GUID
```

#### 3. API-First Event Chain Testing

```typescript
test("doctor appointment event chain", async ({ rabbitmq }) => {
  // 1. Create via GraphQL API (10x faster than UI)
  const result = await client.mutate(CREATE_APPOINTMENT_MUTATION, variables);

  // 2. Verify RabbitMQ event published
  const event = await rabbitmq.waitForMessage(
    (msg) => msg.eventType === "HealthAppointmentScheduled",
    5000
  );

  // 3. Query backend to verify entities created
  const calendarEvents = await client.query(GET_CALENDAR_EVENTS);

  // 4. Spot-check UI (optional)
  await page.goto("/calendar");
  await expect(page.getByText("Doctor: Dr. Smith")).toBeVisible();
});
```

### Running Tests

```bash
# Local development
npm run e2e              # UI mode (interactive debugging)
npm run e2e:headless     # Headless mode
npm run e2e:chromium     # Single browser
npm run e2e:debug        # Debug mode with breakpoints

# CI/CD
npx playwright test      # Runs all tests on 3 browsers
```

### Test Organization

- **family-creation.spec.ts**: Main E2E tests (happy path, validation, errors)
- **accessibility.spec.ts**: WCAG 2.1 AA compliance (axe-core)
- **cross-browser.spec.ts**: Smoke tests (Chromium, Firefox, WebKit)
- **event-chains.spec.ts**: Event chain templates (SKIPPED until Phase 2)

### Zero-Retry Policy

`retries: 0` forces fixing flaky tests immediately (never mask issues with retries).

### Reference

[ADR-004-PLAYWRIGHT-MIGRATION.md](../architecture/ADR-004-PLAYWRIGHT-MIGRATION.md) - Migration rationale, patterns, metrics.

---

## How to Run Architecture Tests

**CRITICAL:** Run architecture tests to validate Clean Architecture layer dependencies and coding patterns.

### Overview

Family Hub uses [NetArchTest](https://github.com/BenMorris/NetArchTest) to enforce architectural rules at test time. These tests validate:

- **Layer dependencies** (Domain should not depend on Application/Persistence/Presentation)
- **CQRS patterns** (Commands, Queries, Handlers implement correct interfaces)
- **DDD patterns** (Aggregates, Events, Repositories follow conventions)
- **Module boundaries** (Auth↔Family isolation)
- **Naming conventions** (I-prefix for interfaces, correct suffixes)

### Quick Start

```bash
# Run all architecture tests
dotnet test src/api/tests/FamilyHub.Tests.Architecture

# Run specific test class
dotnet test --filter "FullyQualifiedName~CleanArchitectureTests"

# Run with verbose output
dotnet test src/api/tests/FamilyHub.Tests.Architecture --logger "console;verbosity=detailed"

# Run from project directory
cd src/api/tests/FamilyHub.Tests.Architecture && dotnet test
```

### Test Categories

| Test File | Purpose | Rules Validated |
|-----------|---------|-----------------|
| `CleanArchitectureTests.cs` | Layer dependencies | Domain→Application→Presentation inward |
| `CqrsPatternTests.cs` | CQRS patterns | Commands, Queries, Handlers implement `IRequest`, `IRequestHandler` |
| `DddPatternTests.cs` | DDD patterns | Aggregates inherit `AggregateRoot<T>`, Events inherit `DomainEvent` |
| `ModuleBoundaryTests.cs` | Module isolation | Auth↔Family domain boundaries |
| `NamingConventionTests.cs` | Naming rules | I-prefix, Command/Query/Input/Event suffixes |

### Clean Architecture Rules

```
┌─────────────────────────────────────┐
│         Presentation Layer          │  ← GraphQL Mutations, Queries, Types
├─────────────────────────────────────┤
│         Application Layer           │  ← Commands, Queries, Handlers, Validators
├─────────────────────────────────────┤
│           Domain Layer              │  ← Aggregates, Value Objects, Events
└─────────────────────────────────────┘
         Dependencies flow INWARD only
```

**Rules enforced:**

- Domain should NOT depend on Application
- Domain should NOT depend on Persistence
- Domain should NOT depend on Presentation
- Application should NOT depend on Presentation
- Application should NOT depend on Persistence implementations

### Example Test

```csharp
[Theory]
[MemberData(nameof(ModuleAssemblies))]
public void DomainLayer_ShouldNotDependOn_ApplicationLayer(string moduleNamespace, Assembly assembly)
{
    // Arrange
    var domainNamespace = $"{moduleNamespace}{TestConstants.DomainLayer}";
    var applicationNamespace = $"{moduleNamespace}{TestConstants.ApplicationLayer}";
    var types = Types.InAssembly(assembly);

    // Act
    var result = types
        .That()
        .ResideInNamespaceStartingWith(domainNamespace)
        .ShouldNot()
        .HaveDependencyOn(applicationNamespace)
        .GetResult();

    // Assert
    result.IsSuccessful.Should().BeTrue(
        because: $"Domain layer ({domainNamespace}) should not depend on Application layer. " +
                 $"Failing types: {FormatFailingTypes(result.FailingTypeNames)}");
}
```

### Interpreting Results

**Passing output:**

```
Passed!  - Failed:     0, Passed:    27, Skipped:     0, Total:    27
```

**Failing output (violation detected):**

```
Failed CleanArchitectureTests.DomainLayer_ShouldNotDependOn_ApplicationLayer
  Expected result.IsSuccessful to be true because Domain layer should not depend
  on Application layer. Failing types: FamilyHub.Modules.Auth.Domain.Entities.User
```

**Common violations and fixes:**

| Violation | Cause | Fix |
|-----------|-------|-----|
| Domain→Application | Domain entity references command/query | Move logic to Application layer |
| Domain→Persistence | Entity references DbContext | Use repository interface in Domain |
| Application→Presentation | Handler references GraphQL type | Return domain result, map in Presentation |

### Known Violations

Some violations are intentional and documented in `ExceptionRegistry.cs`:

```csharp
// Helpers/ExceptionRegistry.cs
public static class ExceptionRegistry
{
    public static readonly KnownViolation[] KnownViolations =
    [
        new KnownViolation(
            RuleName: "ModuleBoundary_AuthShouldNotDependOn_FamilyDomain",
            TypeName: "User",
            Justification: "User.GetRoleInFamily() needs FamilyAggregate parameter. " +
                           "Scheduled for Phase 6 refactoring.",
            TargetResolutionPhase: "Phase 6",
            AddedDate: "2026-01-10",
            AddedBy: "Claude Code"
        )
    ];
}
```

### Negative Testing

The project includes intentional violation fixtures in `FamilyHub.Tests.Architecture.Fixtures` to verify tests actually catch violations:

```
tests/FamilyHub.Tests.Architecture.Fixtures/Violations/
├── CleanArchitecture/     # Domain referencing Application
├── ModuleBoundary/        # Cross-module dependencies
├── DddPatterns/           # Missing AggregateRoot inheritance
├── CqrsPatterns/          # Missing IRequest implementations
└── NamingConventions/     # Incorrect naming
```

### Running in CI

Architecture tests run automatically in CI pipeline:

```yaml
# .github/workflows/ci.yml
- name: Run Architecture Tests
  run: dotnet test src/api/tests/FamilyHub.Tests.Architecture --no-build
```

### Reference

- [ADR-012: Architecture Testing Strategy](../architecture/ADR-012-ARCHITECTURE-TESTING-STRATEGY.md)
- Test location: `src/api/tests/FamilyHub.Tests.Architecture/`
- Fixtures location: `src/api/tests/FamilyHub.Tests.Architecture.Fixtures/`

---

## Git Workflow

### Commit Format

```
<type>(<scope>): <summary> (#<issue>)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
```

**Types:** feat, fix, docs, style, refactor, test, chore

**Examples:**

```
feat(auth): add OAuth 2.0 flow (#42)
fix(calendar): resolve timezone offset bug (#58)
test(family): add creation validation tests (#61)
```

### Branching

See [git-workflow.md](git-workflow.md) for branching strategy and automation.

---

## Automatic Code Formatting

### Hook Configuration

Family Hub uses Claude Code PostToolUse hooks to automatically format code after AI-assisted edits.

**Configured formatters:**

- **TypeScript/JavaScript:** Prettier + ESLint (frontend)
- **C# files:** dotnet format (backend)
- **Markdown files:** markdownlint-cli2 (documentation) ← NEW
- **JSON/YAML files:** Prettier (configuration) ← NEW

**Configuration:** `.claude/settings.json` (committed to git)

**How it works:**

1. Claude edits a file using Edit or Write tool
2. Hook detects file extension
3. Appropriate formatter runs automatically
4. Changes appear in next file read/git diff

**Manual formatting:**

Frontend:

```bash
cd src/frontend/family-hub-web
npm run lint -- --fix
npm run lint:md:fix  # Markdown
npx prettier --write "src/**/*.{ts,js,html,css,scss}"
```

Backend:

```bash
cd src/api
dotnet format
```

Documentation:

```bash
npx markdownlint-cli2 --fix "**/*.md"  # All markdown files
npx prettier --write "**/*.{json,yml,yaml}"  # All config files
```

**Token Savings:**

- Markdownlint: **40% reduction** for documentation-heavy sessions (191 markdown files, 280K words)
- JSON/YAML formatting: Consistent configuration files reduce context noise

**Troubleshooting:**

If hooks fail (rare):

1. Check hook execution: Look for errors in Claude Code output
2. Disable temporarily: Add to `.claude/settings.local.json`:

   ```json
   {
     "hooks": {
       "PostToolUse": []
     }
   }
   ```

3. Re-enable: Remove override from `.local.json`

**Full guide:** [HOOKS.md](HOOKS.md) - Comprehensive hook documentation

---

## Performance Testing with k6

**CRITICAL:** Run performance tests before production deployments to verify API response time targets.

### Overview

Family Hub uses [k6](https://k6.io/) for load and stress testing the GraphQL API. Performance tests validate response time thresholds defined in [Section 12.7](../architecture/MODULAR-DOTNET-HOTCHOCOLATE-GUIDE.md).

### Quick Start

```bash
# Install k6 (macOS)
brew install k6

# Install k6 (Linux/Debian)
sudo apt-get install k6

# Run baseline test
cd tests/performance
k6 run scenarios/baseline.js

# Run load test
k6 run scenarios/load.js

# Run stress test
k6 run scenarios/stress.js
```

### Performance Targets

| Metric | Baseline | Load | Stress |
|--------|----------|------|--------|
| p50 | < 50ms | < 200ms | < 500ms |
| p95 | < 150ms | < 500ms | < 1000ms |
| p99 | < 300ms | < 1000ms | < 3000ms |
| Error Rate | < 0.1% | < 1% | < 5% |

### Test Scenarios

| Scenario | VUs | Duration | Purpose |
|----------|-----|----------|---------|
| Baseline | 10 constant | 1 min | Quick validation, CI smoke test |
| Load | 0→50→100→0 | 10 min | Capacity validation, find bottlenecks |
| Stress | 10→200→10 | 3 min | Find breaking point, verify recovery |

### CI/CD Integration

Performance tests run via GitHub Actions:

- **Manual trigger:** Actions → "Performance Tests (k6)" → Run workflow
- **Nightly schedule:** Automatically at 2 AM UTC
- **Scenarios:** baseline, load, stress, or all

**Workflow:** `.github/workflows/performance.yml`

### Environment Configuration

```bash
# Local development (default)
k6 run scenarios/baseline.js

# Specify environment
k6 run -e K6_ENV=ci scenarios/load.js

# Custom GraphQL URL
k6 run -e GRAPHQL_URL=http://custom:5002/graphql scenarios/baseline.js
```

### Directory Structure

```
tests/performance/
├── config/
│   ├── thresholds.js      # Threshold configurations
│   └── environments.js    # Environment settings
├── helpers/
│   └── graphql.js         # GraphQL request helpers
├── scenarios/
│   ├── baseline.js        # Baseline test
│   ├── load.js            # Load test
│   └── stress.js          # Stress test
└── results/               # Test output (git-ignored)
```

### Full Documentation

See [tests/performance/README.md](../../tests/performance/README.md) for complete k6 documentation including:

- Installation instructions (all platforms)
- Detailed test scenario descriptions
- Writing new tests
- Troubleshooting guide

### Related

- **Issue:** #63 - Create k6 Performance Benchmarking Suite
- **Architecture:** [Section 12.7 - Performance Testing](../architecture/MODULAR-DOTNET-HOTCHOCOLATE-GUIDE.md)
- **DataLoader Guide:** [How to Create DataLoaders](#how-to-create-dataloaders) - Implementation patterns

---

## DataLoader Performance Benchmarks

**CRITICAL:** Run DataLoader benchmarks to validate N+1 query prevention efficiency (ADR-011 targets).

### Overview

DataLoader benchmarks validate the efficiency of Hot Chocolate's GreenDonut DataLoaders for batching and caching GraphQL resolver queries. Without DataLoaders, nested queries cause N+1 performance problems. See [How to Create DataLoaders](#how-to-create-dataloaders) for implementation patterns.

### Expected Performance (ADR-011)

| Metric | Without DataLoaders | With DataLoaders | Improvement |
|--------|---------------------|------------------|-------------|
| Query Count (100 users) | 201 | ≤3 | 67x reduction |
| Latency (p95) | ~8.4s | <200ms | 42x improvement |

### DataLoaders Tested

| DataLoader | Type | Purpose |
|------------|------|---------|
| `UserBatchDataLoader` | 1:1 | Load single user by ID |
| `FamilyBatchDataLoader` | 1:1 | Load single family by ID |
| `UsersByFamilyGroupedDataLoader` | 1:N | Load all members for families |
| `InvitationsByFamilyGroupedDataLoader` | 1:N | Load all invitations for families |

### Quick Start

```bash
# Prerequisites
# 1. API running with Test environment
ASPNETCORE_ENVIRONMENT=Test dotnet run --project src/api/FamilyHub.Api

# 2. Seed test data
cd tests/performance
npm run seed:dataloader
# Or: PGPASSWORD=Dev123! psql -h localhost -U postgres -d familyhub -f seed/dataloader-test-data.sql

# 3. Run benchmark
k6 run scenarios/dataloader.js
```

### Test Scenarios

The DataLoader benchmark runs two phases:

| Phase | VUs | Duration | Purpose |
|-------|-----|----------|---------|
| Baseline | 10 constant | 1 min | Establish baseline metrics |
| Load | 0→30 ramping | 3 min | Validate under moderate load |

### GraphQL Queries Benchmarked

| Query | Description | Expected p95 |
|-------|-------------|--------------|
| `familyWithMembers` | Tests UsersByFamilyGroupedDataLoader | <200ms |
| `familyWithInvitations` | Tests InvitationsByFamilyGroupedDataLoader | <200ms |
| `familyWithOwner` | Tests UserBatchDataLoader | <150ms |
| `familyMembersWithFamilies` | Tests nested DataLoader chains | <300ms |
| `familyComplete` | Tests ALL DataLoaders together | <300ms |

### Test Authentication

DataLoader benchmarks require authenticated requests. In Test environment, authentication is bypassed via HTTP headers:

```javascript
// k6 sends X-Test-User-Id header
const headers = { 'X-Test-User-Id': '00000000-0000-0000-0000-000000000001' };
```

**How it works:**

1. `ASPNETCORE_ENVIRONMENT=Test` activates test auth bypass
2. `TestAuthorizationHandler` succeeds all authorization checks
3. `HeaderBasedCurrentUserService` reads user ID from header
4. k6 sends deterministic test user IDs seeded in database

### CI/CD Integration

DataLoader benchmarks run automatically:

- **Manual trigger:** Actions → "Performance Tests (k6)" → scenario: `dataloader`
- **Nightly schedule:** Included in `all` scenario at 2 AM UTC
- **Data seeding:** Automatic before DataLoader tests

**Workflow:** `.github/workflows/performance.yml`

### Interpreting Results

**Passing thresholds:**

```
✓ http_req_duration{name:family_members}..............: avg=45ms p(95)=120ms
✓ http_req_duration{name:family_complete}.............: avg=80ms p(95)=180ms
✓ checks...............................................: 99.8% ✓ 4990 ✗ 10
```

**Failing thresholds (investigate):**

```
✗ http_req_duration{name:family_members}..............: avg=450ms p(95)=1200ms
    ↳ p(95)<200 .......................... fail
```

**Common issues:**

- **High latency:** DataLoader not registered, query not batching
- **High error rate:** Test data not seeded, auth bypass not active
- **Inconsistent results:** Cold start; run warmup iteration first

### Custom Metrics

The benchmark tracks additional metrics:

- `dl_family_members_duration` - Trend for members query
- `dl_family_invitations_duration` - Trend for invitations query
- `dl_family_owner_duration` - Trend for owner query
- `dl_nested_query_duration` - Trend for nested queries
- `dl_complete_query_duration` - Trend for complete query
- `dl_total_members_loaded` - Counter of total members loaded
- `dl_total_invitations_loaded` - Counter of total invitations loaded

### Full Documentation

See [tests/performance/README.md](../../tests/performance/README.md) for complete documentation including:

- Installation instructions
- Test data seeding
- Troubleshooting guide
- All scenario details

### Related

- **Issue:** #75 - Performance Tests - DataLoader Benchmarks (k6)
- **ADR-011:** [DataLoader Performance Targets](../architecture/ADR-011-DATALOADER-PERFORMANCE.md)
- **Integration Tests:** `DataLoaderQueryCountTests.cs` (validates exact query counts)

---

**Last updated:** 2026-01-13
**Version:** 2.3.0
