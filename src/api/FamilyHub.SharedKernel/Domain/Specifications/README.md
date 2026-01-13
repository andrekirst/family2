# Specification Pattern - API Reference

**Location:** `FamilyHub.SharedKernel.Domain.Specifications`

---

## Interfaces

### ISpecification\<T\>

Core interface for in-memory evaluation.

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

Extends `ISpecification<T>` for EF Core support.

```csharp
public interface IQueryableSpecification<T> : ISpecification<T> where T : class
{
    Expression<Func<T, bool>> ToExpression();
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }
    IReadOnlyList<string> IncludeStrings { get; }
    bool IgnoreQueryFilters { get; }
}
```

### IOrderedSpecification\<T\>

Adds ordering support.

```csharp
public interface IOrderedSpecification<T> : IQueryableSpecification<T> where T : class
{
    IReadOnlyList<OrderExpression<T>> OrderExpressions { get; }
}
```

### IPaginatedSpecification\<T\>

Adds pagination support.

```csharp
public interface IPaginatedSpecification<T> : IOrderedSpecification<T> where T : class
{
    int Skip { get; }
    int Take { get; }
}
```

### IProjectionSpecification\<TSource, TResult\>

Supports select projections.

```csharp
public interface IProjectionSpecification<TSource, TResult>
    : IQueryableSpecification<TSource> where TSource : class
{
    Expression<Func<TSource, TResult>> Projection { get; }
}
```

---

## Base Class

### Specification\<T\>

Abstract base class with lazy compilation caching.

```csharp
public abstract class Specification<T> : IQueryableSpecification<T> where T : class
{
    public abstract Expression<Func<T, bool>> ToExpression();

    // In-memory evaluation (uses cached compiled expression)
    public bool IsSatisfiedBy(T entity);
    public Task<bool> IsSatisfiedByAsync(T entity, CancellationToken ct = default);

    // Result-based evaluation
    public Result<bool> Evaluate(T entity);
    public Task<Result<bool>> EvaluateAsync(T entity, CancellationToken ct = default);

    // Override for eager loading
    public virtual IReadOnlyList<Expression<Func<T, object>>> Includes { get; }
    public virtual IReadOnlyList<string> IncludeStrings { get; }

    // Override for soft-delete bypass
    public virtual bool IgnoreQueryFilters { get; }
}
```

---

## Composite Specifications

### AndSpecification\<T\>

Logical AND of two specifications.

```csharp
var combined = new AndSpecification<User>(spec1, spec2);
// Or via extension: spec1.And(spec2)
// Or via operator: spec1 & spec2
```

### OrSpecification\<T\>

Logical OR of two specifications.

```csharp
var combined = new OrSpecification<User>(spec1, spec2);
// Or via extension: spec1.Or(spec2)
// Or via operator: spec1 | spec2
```

### NotSpecification\<T\>

Logical NOT of a specification.

```csharp
var negated = new NotSpecification<User>(spec);
// Or via extension: spec.Not()
// Or via operator: !spec
```

### RawExpressionSpecification\<T\>

Wraps a raw expression as a specification.

```csharp
var spec = new RawExpressionSpecification<User>(u => u.IsAdmin);
// Or via extension: existingSpec.And(u => u.IsAdmin)
```

---

## Extension Methods

### SpecificationExtensions

```csharp
// Composition
public static IQueryableSpecification<T> And<T>(
    this IQueryableSpecification<T> left,
    IQueryableSpecification<T> right);

public static IQueryableSpecification<T> And<T>(
    this IQueryableSpecification<T> left,
    Expression<Func<T, bool>> right);

public static IQueryableSpecification<T> Or<T>(
    this IQueryableSpecification<T> left,
    IQueryableSpecification<T> right);

public static IQueryableSpecification<T> Or<T>(
    this IQueryableSpecification<T> left,
    Expression<Func<T, bool>> right);

public static IQueryableSpecification<T> Not<T>(
    this IQueryableSpecification<T> spec);

// Ordering
public static IOrderedSpecification<T> OrderBy<T, TKey>(
    this IQueryableSpecification<T> spec,
    Expression<Func<T, TKey>> keySelector);

public static IOrderedSpecification<T> OrderByDescending<T, TKey>(
    this IQueryableSpecification<T> spec,
    Expression<Func<T, TKey>> keySelector);

public static IOrderedSpecification<T> ThenBy<T, TKey>(
    this IOrderedSpecification<T> spec,
    Expression<Func<T, TKey>> keySelector);

public static IOrderedSpecification<T> ThenByDescending<T, TKey>(
    this IOrderedSpecification<T> spec,
    Expression<Func<T, TKey>> keySelector);

// Pagination
public static IPaginatedSpecification<T> Paginate<T>(
    this IOrderedSpecification<T> spec,
    int page,
    int pageSize);
```

---

## Creating Specifications

### Simple Specification

```csharp
public class ActiveUserSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.DeletedAt == null;
}
```

### Parameterized Specification

```csharp
public class UserByEmailSpecification : Specification<User>
{
    private readonly Email _email;

    public UserByEmailSpecification(Email email)
    {
        ArgumentNullException.ThrowIfNull(email);
        _email = email;
    }

    public override Expression<Func<User, bool>> ToExpression()
        => user => user.Email == _email && user.DeletedAt == null;
}
```

### Specification with Includes

```csharp
public class UserWithFamiliesSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.DeletedAt == null;

    public override IReadOnlyList<Expression<Func<User, object>>> Includes
        => [user => user.Families];
}
```

### Specification Ignoring Query Filters

```csharp
public class IncludeSoftDeletedUserSpecification : Specification<User>
{
    private readonly Email _email;

    public IncludeSoftDeletedUserSpecification(Email email) => _email = email;

    public override Expression<Func<User, bool>> ToExpression()
        => user => user.Email == _email;

    public override bool IgnoreQueryFilters => true;
}
```

---

## Usage Examples

### Repository Usage

```csharp
// Find one
var user = await repository.FindOneAsync(
    new UserByEmailSpecification(email),
    cancellationToken);

// Find all
var users = await repository.FindAllAsync(
    new UsersByFamilySpecification(familyId),
    cancellationToken);

// Count
var count = await repository.CountAsync(
    new ActiveUserSpecification(),
    cancellationToken);

// Exists
var exists = await repository.AnyAsync(
    new UserByEmailSpecification(email),
    cancellationToken);
```

### Composition

```csharp
// Extension methods
var spec = new ActiveUserSpecification()
    .And(new UserByEmailSpecification(email));

// Operators
var spec = new ActiveUserSpecification() & new UserByEmailSpecification(email);

// Raw expression
var spec = new ActiveUserSpecification()
    .And(u => u.CreatedAt > DateTime.UtcNow.AddDays(-30));
```

### Ordering and Pagination

```csharp
var spec = new ActiveUserSpecification()
    .OrderBy(u => u.LastName)
    .ThenBy(u => u.FirstName)
    .Paginate(page: 1, pageSize: 20);

var users = await repository.FindAllAsync(spec, ct);
```

---

## File Structure

```
FamilyHub.SharedKernel/Domain/Specifications/
├── ISpecification.cs              # Base interface
├── IQueryableSpecification.cs     # EF Core support
├── IOrderedSpecification.cs       # Ordering support
├── IPaginatedSpecification.cs     # Pagination support
├── IProjectionSpecification.cs    # Projection support
├── Specification.cs               # Abstract base class
├── AndSpecification.cs            # AND composite
├── OrSpecification.cs             # OR composite
├── NotSpecification.cs            # NOT composite
├── RawExpressionSpecification.cs  # Lambda wrapper
├── OrderedSpecification.cs        # Ordered impl
├── PaginatedSpecification.cs      # Paginated impl
├── OrderExpression.cs             # Order expression type
├── SpecificationExtensions.cs     # Extension methods
└── README.md                      # This file
```

---

## Related Components

- `SpecificationEvaluator` - Applies specs to IQueryable
- `ISpecificationRepository<TEntity, TId>` - Repository interface
- `SpecificationDiagnosticEvents` - Diagnostic events
- `Maybe<T>` - Null handling type

---

## Documentation

Full documentation: [docs/development/SPECIFICATION_PATTERN.md](../../../../../docs/development/SPECIFICATION_PATTERN.md)
