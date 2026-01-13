# Specification Pattern

**Last Updated:** 2026-01-13
**Version:** 1.0.0

---

## Overview

The Specification Pattern is a Domain-Driven Design (DDD) pattern that encapsulates business rules into reusable, composable, and testable units. Family Hub uses a custom implementation (no external dependencies) that supports both in-memory evaluation and EF Core IQueryable translation.

### Why Specifications?

1. **Encapsulation:** Business rules live alongside domain entities
2. **Composability:** Combine specifications with `And`, `Or`, `Not`
3. **Testability:** Easy to unit test in isolation
4. **Single Responsibility:** Each specification represents one rule
5. **Reusability:** Use same spec for validation and querying

---

## Quick Start

### Creating a Specification

```csharp
using FamilyHub.SharedKernel.Domain.Specifications;

public class ActiveUserSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.DeletedAt == null;
}
```

### Using a Specification

```csharp
// In-memory evaluation
var spec = new ActiveUserSpecification();
bool isActive = spec.IsSatisfiedBy(user);

// With repository (EF Core)
var activeUsers = await userRepository.FindAllAsync(
    new ActiveUserSpecification(),
    cancellationToken);
```

### Composing Specifications

```csharp
// Using extension methods
var spec = new ActiveUserSpecification()
    .And(new UserByEmailSpecification(email))
    .Or(new AdminUserSpecification());

// Using operators
var spec = activeSpec & emailSpec | adminSpec;

// Using raw expressions
var spec = new ActiveUserSpecification()
    .And(u => u.Email.Value.EndsWith("@company.com"));
```

---

## Architecture

### Layer Placement

Specifications reside in the **Domain Layer** alongside aggregates:

```
Modules/FamilyHub.Modules.Auth/
├── Domain/
│   ├── Entities/
│   │   └── User.cs
│   ├── Specifications/          ← Specifications here
│   │   ├── UserByEmailSpecification.cs
│   │   └── UsersByFamilySpecification.cs
│   └── Repositories/
│       └── IUserRepository.cs
```

### Interface Hierarchy

```
ISpecification<T>                    ← In-memory evaluation
    ↓
IQueryableSpecification<T>           ← EF Core IQueryable support
    ↓
IOrderedSpecification<T>             ← Sorting support
    ↓
IPaginatedSpecification<T>           ← Pagination support

IProjectionSpecification<T, TResult> ← Select projections
```

---

## Core Components

### ISpecification\<T\>

Base interface for all specifications:

```csharp
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
    Task<bool> IsSatisfiedByAsync(T entity, CancellationToken ct = default);
    Result<bool> Evaluate(T entity);
    Task<Result<bool>> EvaluateAsync(T entity, CancellationToken ct = default);
}
```

### IQueryableSpecification\<T\>

Adds EF Core support:

```csharp
public interface IQueryableSpecification<T> : ISpecification<T> where T : class
{
    Expression<Func<T, bool>> ToExpression();
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }
    IReadOnlyList<string> IncludeStrings { get; }
    bool IgnoreQueryFilters { get; }
}
```

### Specification\<T\> Base Class

Abstract base providing common functionality:

```csharp
public abstract class Specification<T> : IQueryableSpecification<T> where T : class
{
    // Lazy compilation cache
    private readonly Lazy<Func<T, bool>> _compiledExpression;

    // Override this in derived classes
    public abstract Expression<Func<T, bool>> ToExpression();

    // Evaluates using cached compiled expression
    public bool IsSatisfiedBy(T entity) => _compiledExpression.Value(entity);
}
```

---

## Repository Integration

### ISpecificationRepository\<TEntity, TId\>

Repository interface extension:

```csharp
public interface ISpecificationRepository<TEntity, TId>
{
    Task<TEntity?> FindOneAsync(
        IQueryableSpecification<TEntity> specification,
        CancellationToken ct = default);

    Task<List<TEntity>> FindAllAsync(
        IQueryableSpecification<TEntity> specification,
        CancellationToken ct = default);

    Task<int> CountAsync(
        IQueryableSpecification<TEntity> specification,
        CancellationToken ct = default);

    Task<bool> AnyAsync(
        IQueryableSpecification<TEntity> specification,
        CancellationToken ct = default);

    Task<List<TResult>> FindAllProjectedAsync<TResult>(
        IProjectionSpecification<TEntity, TResult> specification,
        CancellationToken ct = default);
}
```

### SpecificationEvaluator

Applies specifications to IQueryable:

```csharp
public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery,
        IQueryableSpecification<T> specification) where T : class
    {
        var query = inputQuery;

        // Apply query filters override
        if (specification.IgnoreQueryFilters && query is IQueryable<T> q)
            query = q.IgnoreQueryFilters();

        // Apply includes
        foreach (var include in specification.Includes)
            query = query.Include(include);

        foreach (var includeString in specification.IncludeStrings)
            query = query.Include(includeString);

        // Apply where clause
        query = query.Where(specification.ToExpression());

        return query;
    }
}
```

---

## Composition Patterns

### Fluent Extension Methods

```csharp
// And composition
var spec = activeSpec.And(emailSpec);

// Or composition
var spec = adminSpec.Or(ownerSpec);

// Not composition
var spec = activeSpec.Not();

// Chaining
var spec = activeSpec.And(emailSpec).Or(adminSpec).Not();
```

### Operator Overloads

```csharp
// Bitwise AND for And
var spec = activeSpec & emailSpec;

// Bitwise OR for Or
var spec = adminSpec | ownerSpec;

// Logical NOT for Not
var spec = !activeSpec;

// Complex composition
var spec = (activeSpec & emailSpec) | (!deletedSpec);
```

### Raw Expression Composition

```csharp
// Add lambda directly
var spec = new ActiveUserSpecification()
    .And(u => u.CreatedAt > DateTime.UtcNow.AddDays(-30));

// With Or
var spec = new AdminUserSpecification()
    .Or(u => u.FamilyIds.Contains(familyId));
```

---

## Advanced Features

### Eager Loading (Includes)

```csharp
public class UserWithFamiliesSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
        => u => u.DeletedAt == null;

    public override IReadOnlyList<Expression<Func<User, object>>> Includes
        => [u => u.Families];
}
```

### Soft Delete Override

```csharp
public class IncludeSoftDeletedUserSpecification : Specification<User>
{
    private readonly Email _email;

    public IncludeSoftDeletedUserSpecification(Email email) => _email = email;

    public override Expression<Func<User, bool>> ToExpression()
        => u => u.Email == _email;

    // Override to include soft-deleted entities
    public override bool IgnoreQueryFilters => true;
}
```

### Ordering

```csharp
var orderedSpec = new ActiveUserSpecification()
    .OrderBy(u => u.CreatedAt)
    .ThenByDescending(u => u.LastLoginAt);

// With repository
var users = await repository.FindAllAsync(orderedSpec, ct);
```

### Pagination

```csharp
var pagedSpec = new ActiveUserSpecification()
    .OrderBy(u => u.CreatedAt)
    .Paginate(page: 1, pageSize: 20);

// With repository
var users = await repository.FindAllAsync(pagedSpec, ct);
```

### Projections

```csharp
public class UserSummaryProjection
    : IProjectionSpecification<User, UserSummaryDto>
{
    public Expression<Func<User, bool>> ToExpression()
        => u => u.DeletedAt == null;

    public Expression<Func<User, UserSummaryDto>> Projection
        => u => new UserSummaryDto(u.Id, u.Email, u.DisplayName);
}

// Usage
var summaries = await repository.FindAllProjectedAsync(
    new UserSummaryProjection(),
    ct);
```

---

## Existing Specifications

### Auth Module

| Specification | Purpose |
|---------------|---------|
| `UserByEmailSpecification` | Find user by email |
| `UserByIdSpecification` | Find user by ID |
| `UserByExternalProviderSpecification` | Find user by OAuth provider |
| `UsersByFamilySpecification` | Find all users in a family |
| `IncludeSoftDeletedUserSpecification` | Find user including soft-deleted |

### Family Module

| Specification | Purpose |
|---------------|---------|
| `PendingInvitationSpecification` | Find pending invitations |
| `PendingInvitationByFamilySpecification` | Pending invitations for family |
| `PendingInvitationByEmailSpecification` | Pending invitation by email |
| `ExpiredInvitationForCleanupSpecification` | Expired invitations for cleanup |
| `InvitationByTokenSpecification` | Find invitation by token |
| `InvitationByIdSpecification` | Find invitation by ID |

---

## Testing

### Test Fixtures

```csharp
// Extension method approach
entity.ShouldSatisfy(spec);
entity.ShouldNotSatisfy(spec);

// Multiple specifications
entity.ShouldSatisfyAll(spec1, spec2, spec3);
entity.ShouldSatisfyAny(spec1, spec2, spec3);

// Expression validation
spec.ShouldHaveValidExpression();
```

### Fixture-Based Testing

```csharp
// Create fixture with test entities
var fixture = SpecificationTestExtensions.CreateSpecificationFixture(
    user1, user2, user3, deletedUser);

// Assert exact matches
fixture.ShouldMatchExactly(spec, user1, user2);

// Assert count
fixture.ShouldMatchCount(spec, 2);
```

### Example Test

```csharp
public class UserByEmailSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_MatchingEmail_ReturnsTrue()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var user = new UserBuilder().WithEmail(email).Build();
        var spec = new UserByEmailSpecification(email);

        // Act & Assert
        user.ShouldSatisfy(spec);
    }

    [Fact]
    public void ToExpression_ProducesValidExpression()
    {
        // Arrange
        var spec = new UserByEmailSpecification(Email.From("test@example.com"));

        // Act & Assert
        spec.ShouldHaveValidExpression();
    }
}
```

---

## Migration from Legacy Methods

Legacy repository methods are marked `[Obsolete]` with guidance:

```csharp
// Before (deprecated)
var user = await userRepository.GetByEmailAsync(email, ct);

// After (specification-based)
var user = await userRepository.FindOneAsync(
    new UserByEmailSpecification(email),
    ct);
```

### Migration Steps

1. Identify calls to obsolete methods (compiler warnings)
2. Create or use existing specification
3. Replace with `FindOneAsync`/`FindAllAsync`
4. Run tests to verify behavior

---

## Naming Convention

Follow the pattern: `{Condition}{Entity}Specification`

| Example | Meaning |
|---------|---------|
| `ActiveUserSpecification` | Users that are active |
| `PendingInvitationSpecification` | Invitations that are pending |
| `UserByEmailSpecification` | User lookup by email |
| `ExpiredInvitationForCleanupSpecification` | Expired invitations ready for cleanup |

---

## Diagnostics

Specifications emit diagnostic events for observability:

```csharp
// Event data
public record SpecificationDiagnosticData(
    string SpecificationName,
    string EntityType,
    TimeSpan Duration,
    int MatchCount);

// Subscribe in DI setup
services.AddSpecificationDiagnostics();
```

---

## Best Practices

1. **Keep specifications focused** - One rule per specification
2. **Use composition** - Combine simple specs instead of complex expressions
3. **Test expressions** - Verify expressions compile and translate to SQL
4. **Name descriptively** - Name reflects the business rule
5. **Document non-obvious specs** - Add XML comments for complex logic
6. **Use IgnoreQueryFilters sparingly** - Only for admin scenarios
7. **Prefer specifications over raw queries** - Encapsulate all query logic

---

## Related Documentation

- **Coding Standards:** [CODING_STANDARDS.md](CODING_STANDARDS.md)
- **Patterns:** [PATTERNS.md](PATTERNS.md)
- **Backend Guide:** [src/api/CLAUDE.md](../../src/api/CLAUDE.md)
- **Testing:** [WORKFLOWS.md](WORKFLOWS.md)
