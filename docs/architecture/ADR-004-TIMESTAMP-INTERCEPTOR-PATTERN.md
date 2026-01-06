# ADR-004: Automatic Timestamp Management with EF Core Interceptor

**Status:** Accepted
**Date:** 2026-01-03
**Deciders:** Andre Kirst (with Claude Code AI)
**Tags:** ef-core, persistence, infrastructure, timestamps, testing

## Context

Family Hub tracks creation and modification timestamps (`CreatedAt`, `UpdatedAt`) for all entities to support audit trails, data synchronization, and troubleshooting. Initially, timestamps were manually managed in domain methods using `DateTime.UtcNow`, but this approach had significant drawbacks.

### Problem Statement

**Manual Timestamp Management Issues:**

1. **Inconsistency Risk**: Developers could forget to set `UpdatedAt` in domain methods
2. **Verbose Boilerplate**: Every entity modification required `UpdatedAt = DateTime.UtcNow`
3. **Testing Complexity**: Domain tests needed to verify timestamp updates, mixing infrastructure concerns with domain logic
4. **Time Coupling**: Domain methods were coupled to system time, making deterministic testing difficult

**Example of Manual Approach (Before):**

```csharp
public class Family : AggregateRoot<FamilyId>
{
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Family(FamilyId id, FamilyName name) : base(id)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;  // Manual
        UpdatedAt = DateTime.UtcNow;  // Manual
    }

    public void UpdateName(FamilyName newName)
    {
        Name = newName;
        UpdatedAt = DateTime.UtcNow;  // Easy to forget!
    }
}
```

**Problems:**

- If a developer added a new mutation method and forgot `UpdatedAt = DateTime.UtcNow`, the timestamp would be stale
- Tests had to assert on timestamps, which is an infrastructure concern, not domain logic
- Mocking `DateTime.UtcNow` for tests required workarounds

### Technology Stack Context

- **Entity Framework Core 10.0** - ORM with interceptor support
- **.NET 10 / C# 14** - `TimeProvider` abstraction introduced in .NET 8
- **PostgreSQL 16** - Database with `CURRENT_TIMESTAMP` defaults
- **xUnit + FluentAssertions** - Test framework
- **Vogen 8.0+** - Strongly-typed value objects for IDs

### Architectural Context

Family Hub follows **Domain-Driven Design (DDD)** with:

- **Entity Base Class**: All entities inherit from `Entity<TId>` or `AggregateRoot<TId>`
- **Value Objects**: Timestamps are NOT value objects (they're infrastructure metadata)
- **Bounded Contexts**: Modular monolith with 8 modules, each with its own DbContext
- **Event-Driven**: Domain events tracked, but timestamps are separate from domain events

## Decision

**We will use an EF Core Interceptor with TimeProvider dependency injection for automatic timestamp management.**

### Architecture

#### 1. Marker Interfaces

```csharp
/// <summary>
/// Marker interface for entities requiring automatic timestamp management.
/// </summary>
public interface ITimestampable
{
    DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Marker interface for entities supporting soft deletion.
/// </summary>
public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
}
```

**Design Notes:**

- `public set` required for EF Core interceptor access (C# interfaces cannot have `internal set`)
- Protection achieved through `protected` constructors and domain method discipline
- `ISoftDeletable` separate because not all entities need soft delete

#### 2. Entity Base Class Updates

```csharp
public abstract class Entity<TId> : IEquatable<Entity<TId>>, ITimestampable
    where TId : notnull
{
    public TId Id { get; }

    /// <summary>
    /// When the entity was created. Set automatically on first save.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the entity was last updated. Set automatically on every save.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    protected Entity(TId id) { Id = id; }
    protected Entity() : this(default!) { } // EF Core constructor
}
```

**Design Decision:** All entities get timestamps for consistency. No separate `AuditableEntity` layer needed.

#### 3. TimestampInterceptor

```csharp
public sealed class TimestampInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    public TimestampInterceptor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void UpdateTimestamps(DbContext? context)
    {
        if (context == null) return;

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var timestampableEntries = context.ChangeTracker
            .Entries<ITimestampable>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in timestampableEntries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Set both timestamps on creation
                    entry.Entity.UpdatedAt = now;
                    var createdAtProperty = entry.Property(nameof(Entity<object>.CreatedAt));
                    createdAtProperty.CurrentValue = now;
                    break;

                case EntityState.Modified:
                    // Only update UpdatedAt on modification
                    entry.Entity.UpdatedAt = now;
                    entry.Property(nameof(Entity<object>.CreatedAt)).IsModified = false;
                    break;
            }
        }
    }
}
```

**Key Features:**

- Uses `TimeProvider` for testability (can inject `FakeTimeProvider`)
- Strongly-typed: Explicit casting to `Entity<object>` for compile-time safety
- Prevents `CreatedAt` modification via `IsModified = false`
- Processes only `ITimestampable` entities

#### 4. Registration Pattern

```csharp
// DI Registration
services.AddSingleton(TimeProvider.System);

// DbContext Configuration Extension
public static class DbContextOptionsExtensions
{
    public static DbContextOptionsBuilder AddTimestampInterceptor(
        this DbContextOptionsBuilder optionsBuilder,
        IServiceProvider serviceProvider)
    {
        var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        var interceptor = new TimestampInterceptor(timeProvider);
        return optionsBuilder.AddInterceptors(interceptor);
    }
}

// Usage in Module Registration
services.AddPooledDbContextFactory<AuthDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString)
        .AddTimestampInterceptor(sp);
});
```

#### 5. Domain Entity Updates (After)

```csharp
public class Family : AggregateRoot<FamilyId>, ISoftDeletable
{
    // Timestamps inherited from Entity<FamilyId>
    // No manual timestamp management!

    public DateTime? DeletedAt { get; set; }

    private Family(FamilyId id, FamilyName name) : base(id)
    {
        Name = name;
        // REMOVED: CreatedAt = DateTime.UtcNow;
        // REMOVED: UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(FamilyName newName)
    {
        Name = newName;
        // REMOVED: UpdatedAt = DateTime.UtcNow;
        // Interceptor handles this automatically!
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow; // Business timestamp - keep manual
        // REMOVED: UpdatedAt = DateTime.UtcNow; // Interceptor handles
    }
}
```

#### 6. GraphQL Exposure Pattern

Timestamps are abstracted into a dedicated `AuditInfoType`:

```csharp
public sealed record AuditInfoType
{
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}

public class FamilyType : ObjectType<Family>
{
    protected override void Configure(IObjectTypeDescriptor<Family> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field("auditInfo")
            .Type<NonNullType<ObjectType<AuditInfoType>>>()
            .Description("Audit metadata")
            .Resolve(ctx =>
            {
                var family = ctx.Parent<Family>();
                return new AuditInfoType
                {
                    CreatedAt = family.CreatedAt,
                    UpdatedAt = family.UpdatedAt
                };
            });

        // CreatedAt, UpdatedAt, DeletedAt NOT directly exposed
    }
}
```

## Consequences

### Benefits

✅ **Consistency Guaranteed**: Impossible to forget updating timestamps
✅ **Cleaner Domain Code**: Domain methods focus on business logic, not infrastructure
✅ **Better Testability**: Domain tests don't need to assert on timestamps
✅ **Deterministic Tests**: `FakeTimeProvider` enables time-controlled testing
✅ **Centralized Logic**: One place to modify timestamp behavior for all entities
✅ **Database Safety Net**: PostgreSQL `CURRENT_TIMESTAMP` defaults as fallback
✅ **Type Safety**: Strongly-typed interceptor with compile-time checks

### Trade-offs

⚠️ **Slight Overhead**: Interceptor runs on every save (~1-5ms per operation)
⚠️ **Indirection**: Timestamp logic is "hidden" from domain methods (but well-documented)
⚠️ **Change Tracking Dependency**: Relies on EF Core change tracker accuracy
⚠️ **Public Setters**: `CreatedAt` and `UpdatedAt` have public setters (mitigated by protected constructors)

### Risks & Mitigations

| Risk | Severity | Mitigation |
|------|----------|------------|
| Infinite loops from change tracking | Medium | `IsModified = false` for `CreatedAt` |
| Test failures from timezone issues | Low | Always use UTC times via `TimeProvider.GetUtcNow()` |
| Performance degradation | Low | Benchmarked at ~1-5ms overhead, acceptable |
| Breaking existing data | Low | Phase 0 only, no production data exists |

## Implementation Notes

### Testing Strategy

**Domain Tests**: Remove timestamp assertions (infrastructure concern)

```csharp
[Fact]
public void UpdateName_WithValidName_ShouldUpdateName()
{
    // Arrange
    var family = Family.Create(FamilyName.From("Original"), UserId.New());

    // Act
    family.UpdateName(FamilyName.From("Updated"));

    // Assert
    family.Name.Value.Should().Be("Updated");
    // REMOVED: family.UpdatedAt assertion
}
```

**Integration Tests**: Use `FakeTimeProvider` for time control

```csharp
public class TimestampInterceptorTests
{
    private FakeTimeProvider _timeProvider;

    [Fact]
    public async Task SaveChanges_ModifiedFamily_ShouldUpdateOnlyUpdatedAt()
    {
        // Arrange
        var family = Family.Create(FamilyName.From("Original"), UserId.New());
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var createdAt = family.CreatedAt;

        // Advance time
        _timeProvider.Advance(TimeSpan.FromHours(1));
        var updateTime = _timeProvider.GetUtcNow().UtcDateTime;

        // Act
        family.UpdateName(FamilyName.From("Updated"));
        await _context.SaveChangesAsync();

        // Assert
        family.CreatedAt.Should().Be(createdAt); // Unchanged
        family.UpdatedAt.Should().Be(updateTime); // Updated
    }
}
```

### Database Schema

PostgreSQL columns retain `CURRENT_TIMESTAMP` defaults as safety net:

```sql
CREATE TABLE auth.families (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    owner_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
);
```

If the interceptor fails, PostgreSQL provides default timestamps.

### Future Modules

All future modules should follow this pattern:

1. Register `TimeProvider.System` in module DI (if not already global)
2. Add `.AddTimestampInterceptor(sp)` to DbContext configuration
3. Extend `Entity<TId>` or `AggregateRoot<TId>` (automatic timestamps)
4. Add `ISoftDeletable` for entities needing soft delete
5. Do NOT manually set `CreatedAt` or `UpdatedAt` in domain methods

### Business vs. Infrastructure Timestamps

**Automatic (Infrastructure):**

- `CreatedAt` - Record creation time
- `UpdatedAt` - Last modification time

**Manual (Domain Logic):**

- `EmailVerifiedAt` - When user verified their email (business event)
- `DeletedAt` - Soft delete timestamp (explicit domain action)
- `LastLoginAt` - User authentication tracking

## Alternatives Considered

### Alternative 1: Database Triggers

**Approach:** Use PostgreSQL triggers to update `updated_at`.

**Rejected Because:**

- Doesn't work with in-memory or SQLite tests
- Database-specific logic (breaks DB portability)
- No control over timestamps in tests (can't use `FakeTimeProvider`)

### Alternative 2: Base Class with OnSave Hook

**Approach:** Add `protected virtual void OnBeforeSave()` to `Entity<TId>`.

**Rejected Because:**

- Requires developers to remember to call `base.OnBeforeSave()`
- Doesn't prevent manual `UpdatedAt` assignments
- Less explicit than interceptor pattern

### Alternative 3: Repository Pattern with Timestamp Logic

**Approach:** Repositories set timestamps before `SaveChanges`.

**Rejected Because:**

- Couples timestamp logic to repository implementations
- Harder to test repository behavior in isolation
- More verbose than interceptor

### Alternative 4: Keep Manual Timestamps

**Approach:** Continue using `DateTime.UtcNow` in domain methods.

**Rejected Because:**

- Primary motivation for this ADR was to eliminate manual timestamp management
- Inconsistency risk remains
- Verbose boilerplate in every mutation

## References

- [EF Core Interceptors Documentation](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
- [TimeProvider Abstraction (.NET 8+)](https://learn.microsoft.com/en-us/dotnet/api/system.timeprovider)
- [FakeTimeProvider for Testing](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.time.testing.faketimeprovider)
- [DDD Entity Design](https://martinfowler.com/bliki/EvansClassification.html)
- Original Discussion: Phase 6 timestamp refactoring implementation (2026-01-03)

## Revision History

| Date | Version | Changes |
|------|---------|---------|
| 2026-01-03 | 1.0 | Initial ADR - Timestamp Interceptor Pattern with TimeProvider DI |
