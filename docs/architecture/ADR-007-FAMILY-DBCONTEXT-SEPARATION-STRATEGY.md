# ADR-007: Family DbContext Separation Strategy

**Status:** Accepted
**Date:** 2026-01-12
**Deciders:** Andre Kirst (with Claude Code AI)
**Tags:** persistence, dbcontext, bounded-context, postgresql, efcore, modular-monolith
**Related ADRs:** [ADR-001](ADR-001-MODULAR-MONOLITH-FIRST.md), [ADR-005](ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md), [ADR-011](ADR-011-DATALOADER-PATTERN.md)
**Issue:** #76

## Context

Family Hub follows a **modular monolith** architecture (per [ADR-001](ADR-001-MODULAR-MONOLITH-FIRST.md)) with 8 DDD bounded contexts. During Phase 5 of the Family Module Extraction (per [ADR-005](ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md)), the persistence layer required separation to enforce bounded context boundaries at the database level.

### Problem Statement

Initially, all entities (User, Family, FamilyMemberInvitation) resided in a single `AuthDbContext`. This violated DDD principles:

1. **Mixed Concerns**: User authentication and family management queries interleaved
2. **Unclear Ownership**: No clear aggregate root ownership per bounded context
3. **Migration Coupling**: Schema changes in one domain affected the other
4. **Microservices Blocker**: Single DbContext prevents future service extraction

### Technology Stack

- **EF Core 10**: ORM with pooled DbContext factories
- **PostgreSQL 16**: Primary database with schema-based multi-tenancy
- **Npgsql**: PostgreSQL driver with snake_case naming convention
- **.NET 10 / C# 14**: Target framework
- **Vogen 8.0+**: Strongly-typed value objects (UserId, FamilyId, Email)

### Architecture Before Separation

```
┌─────────────────────────────────────────────────────────────┐
│ AuthDbContext (Single Context)                              │
├─────────────────────────────────────────────────────────────┤
│ Schema: "auth"                                              │
│                                                             │
│ Entities:                                                   │
│   - User (auth + family membership)                         │
│   - Family (family management)                              │
│   - FamilyMemberInvitation (invitation workflow)            │
│   - OutboxEvent (domain events)                             │
│                                                             │
│ Problems:                                                   │
│   ❌ Mixed domain concerns                                  │
│   ❌ Unclear aggregate ownership                            │
│   ❌ Coupled migrations                                     │
│   ❌ No microservices path                                  │
└─────────────────────────────────────────────────────────────┘
```

## Decision

**Implement one DbContext per module** with PostgreSQL schema separation, using cross-module ID references without foreign key constraints and an `IUserLookupService` abstraction for cross-module queries.

### Target Architecture

```
┌─────────────────────────────────────────────────────────────┐
│ PostgreSQL Database: familyhub                              │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ ┌─────────────────────────┐  ┌─────────────────────────┐   │
│ │ AuthDbContext           │  │ FamilyDbContext         │   │
│ │ Schema: "auth"          │  │ Schema: "family"        │   │
│ ├─────────────────────────┤  ├─────────────────────────┤   │
│ │ Entities:               │  │ Entities:               │   │
│ │   - User                │  │   - Family              │   │
│ │   - OutboxEvent         │  │   - FamilyMemberInvit.  │   │
│ │                         │  │                         │   │
│ │ Owns:                   │  │ Owns:                   │   │
│ │   - User authentication │  │   - Family aggregate    │   │
│ │   - User.FamilyId (ref) │  │   - Invitation workflow │   │
│ └─────────────────────────┘  └─────────────────────────┘   │
│                                                             │
│ Cross-Module Communication:                                 │
│   Family.OwnerId ─────────────▶ auth.users.id (ID only)    │
│   FamilyMemberInvitation.InvitedByUserId ──▶ auth.users.id │
│   User.FamilyId ◀─────────────── family.families.id        │
│                                                             │
│ Query Abstraction: IUserLookupService (SharedKernel)       │
└─────────────────────────────────────────────────────────────┘
```

### Implementation Pattern

**FamilyDbContext (Family Module)**

```csharp
public class FamilyDbContext(DbContextOptions<FamilyDbContext> options) : DbContext(options)
{
    public DbSet<Family> Families => Set<Family>();
    public DbSet<FamilyMemberInvitation> FamilyMemberInvitations => Set<FamilyMemberInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set PostgreSQL schema for this module
        modelBuilder.HasDefaultSchema("family");

        // Apply all configurations from this assembly (auto-discovery)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FamilyDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
```

**AuthDbContext (Auth Module)**

```csharp
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
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

**IUserLookupService (SharedKernel)**

```csharp
/// <summary>
/// Cross-module service interface for user lookup operations.
/// Implemented by Auth module, consumed by Family module.
/// Lives in SharedKernel to avoid circular dependencies.
/// </summary>
public interface IUserLookupService
{
    /// <summary>
    /// Gets the FamilyId for a given user.
    /// </summary>
    Task<FamilyId?> GetUserFamilyIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts users belonging to a family.
    /// </summary>
    Task<int> GetFamilyMemberCountAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with the given email is a member of the family.
    /// </summary>
    Task<bool> IsEmailMemberOfFamilyAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default);
}
```

**Pooled DbContext Factory Registration**

```csharp
// FamilyModuleServiceRegistration.cs
services.AddPooledDbContextFactory<FamilyDbContext>((sp, options) =>
{
    var connectionString = configuration.GetConnectionString("FamilyHubDb");
    options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(FamilyDbContext).Assembly.GetName().Name);
        })
        .UseSnakeCaseNamingConvention()
        .AddTimestampInterceptor(sp);
});

// Scoped DbContext from factory
services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IDbContextFactory<FamilyDbContext>>();
    return factory.CreateDbContext();
});
```

## Rationale

### Why Schema-Based Separation (Not Database-Per-Module)

| Aspect | Schema Separation | Database-Per-Module |
|--------|------------------|---------------------|
| **Complexity** | Low (same connection) | High (multiple connections) |
| **Transactions** | Cross-schema possible | Requires distributed tx |
| **Operations** | Single backup/restore | Multiple databases |
| **Migration Path** | Easy to split later | Already split |
| **Phase 0-4 Fit** | ✅ Optimal | ❌ Over-engineered |

**Decision**: Schema separation provides bounded context boundaries without distributed systems complexity. Physical database separation is deferred to Phase 5+ microservices migration.

### Why No Foreign Key Constraints

Cross-schema foreign keys create tight coupling:

1. **Deployment Coupling**: Cannot deploy modules independently
2. **Migration Order**: FK requires target table to exist first
3. **Cascade Risks**: Deletes cascade across module boundaries
4. **Microservices Blocker**: FKs cannot span services

**Solution**: Store only IDs (UserId, FamilyId) without FK constraints. Referential integrity enforced at application level via domain validation.

### Why IUserLookupService Abstraction

Direct cross-module entity access violates bounded context principles:

```
❌ WRONG: Family module directly queries AuthDbContext
   - Creates circular dependency
   - Leaks Auth internal implementation
   - Cannot evolve independently

✅ RIGHT: Family module uses IUserLookupService interface
   - Interface in SharedKernel (neutral)
   - Auth implements (owns data)
   - Family consumes (needs lookup)
   - Returns only primitives/value objects
```

### Why Pooled DbContext Factory

DataLoaders (per [ADR-011](ADR-011-DATALOADER-PATTERN.md)) require DbContext instances outside the request scope:

1. **Lifetime Mismatch**: DataLoader batches outlive request scope
2. **Concurrency**: Multiple async operations need separate contexts
3. **Performance**: Pooling reduces allocation overhead

```csharp
// DataLoader uses factory, not scoped DbContext
public sealed class FamilyBatchDataLoader : BatchDataLoader<FamilyId, Family>
{
    private readonly IDbContextFactory<FamilyDbContext> _dbContextFactory;

    protected override async Task<IReadOnlyDictionary<FamilyId, Family>> LoadBatchAsync(
        IReadOnlyList<FamilyId> keys,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Families
            .Where(f => keys.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, cancellationToken);
    }
}
```

## Alternatives Considered

### Alternative 1: Single DbContext with Entity Tagging

**Approach**: Keep single DbContext, tag entities by module.

```csharp
public class AppDbContext : DbContext
{
    [Module("Auth")] public DbSet<User> Users { get; }
    [Module("Family")] public DbSet<Family> Families { get; }
}
```

**Rejected Because**:

- No schema separation (all in same namespace)
- Cannot enforce ownership at compile time
- Migrations still coupled
- No clear microservices path

### Alternative 2: Separate Databases Per Module

**Approach**: Each module has its own PostgreSQL database.

```
familyhub_auth → AuthDbContext
familyhub_family → FamilyDbContext
```

**Rejected Because**:

- Premature for Phase 0-4 (modular monolith)
- Requires distributed transactions for cross-module operations
- Operational complexity (multiple backups, restores)
- Single developer overhead too high

### Alternative 3: Event Sourcing for Cross-Module Data

**Approach**: Modules maintain read models from domain events.

```csharp
// Family module subscribes to UserCreatedEvent
// Maintains local UserReadModel for lookups
```

**Deferred Because**:

- Significant complexity increase
- Eventual consistency challenges
- Better suited for Phase 5+ microservices
- Can migrate to this approach later

## Consequences

### Positive

1. **Bounded Context Enforcement**: Schema separation enforces module boundaries at database level
2. **Independent Evolution**: Each module's schema can evolve independently
3. **Microservices Ready**: Clean separation enables future service extraction
4. **Performance**: Pooled DbContext factories optimize GraphQL resolver performance
5. **Type Safety**: Vogen value objects (UserId, FamilyId) prevent ID confusion across modules

### Negative

1. **No Referential Integrity**: Cross-module FK constraints removed; application must validate
2. **Query Complexity**: Cross-module queries require IUserLookupService abstraction
3. **Eventual Consistency**: Some operations may require eventual consistency patterns
4. **Multiple Migrations**: Each module has separate migration history

### Mitigation Strategies

| Risk | Mitigation |
|------|------------|
| Orphaned References | Domain validation on write operations |
| Query Performance | IUserLookupService with caching potential |
| Migration Coordination | CI/CD applies all migrations in order |
| Data Inconsistency | Outbox pattern for cross-module events |

## Implementation

### Files Created

| File | Purpose |
|------|---------|
| `Modules/FamilyHub.Modules.Family/Persistence/FamilyDbContext.cs` | Family module DbContext |
| `FamilyHub.SharedKernel/Application/Abstractions/IUserLookupService.cs` | Cross-module query interface |
| `Modules/FamilyHub.Modules.Auth/Application/Services/UserLookupService.cs` | IUserLookupService implementation |

### Files Modified

| File | Change |
|------|--------|
| `Modules/FamilyHub.Modules.Auth/Persistence/AuthDbContext.cs` | Removed Family entities |
| `Modules/FamilyHub.Modules.Family/FamilyModuleServiceRegistration.cs` | Pooled factory registration |
| `Modules/FamilyHub.Modules.Auth/AuthModuleServiceRegistration.cs` | IUserLookupService registration |

### Verification

1. **Build**: `dotnet build` completes without errors
2. **Migrations**: Both Auth and Family migrations apply successfully
3. **Schema Check**: PostgreSQL shows separate `auth` and `family` schemas
4. **Integration Tests**: Cross-module queries via IUserLookupService work correctly

## Related Decisions

- [ADR-001: Modular Monolith First](ADR-001-MODULAR-MONOLITH-FIRST.md) - Overall architecture strategy
- [ADR-005: Family Module Extraction Pattern](ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md) - Extraction process that created this separation
- [ADR-011: DataLoader Pattern](ADR-011-DATALOADER-PATTERN.md) - Uses pooled DbContext factories

## Future Work

- **Phase 5+**: Extract Family module to separate service with own database
- **Event Sourcing**: Consider for cross-module data synchronization
- **Read Models**: Implement CQRS read models for complex cross-module queries

## References

- [EF Core DbContext Pooling](https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics#dbcontext-pooling)
- [PostgreSQL Schemas](https://www.postgresql.org/docs/16/ddl-schemas.html)
- [DDD Bounded Contexts](https://martinfowler.com/bliki/BoundedContext.html)
- [Family Hub Architecture Guide](MODULAR-DOTNET-HOTCHOCOLATE-GUIDE.md)

---

**Decision**: Implement one DbContext per module with PostgreSQL schema separation, cross-module ID references (no FK constraints), and IUserLookupService abstraction for cross-module queries. This enforces bounded context boundaries while maintaining operational simplicity for the modular monolith phase.
