using System.Linq.Expressions;

namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Abstract base class for specifications with lazy expression compilation caching.
/// Provides operator overloads and virtual methods for customization.
/// </summary>
/// <typeparam name="T">The entity type this specification applies to.</typeparam>
public abstract class Specification<T> : IQueryableSpecification<T>
    where T : class
{
    private readonly Lazy<Func<T, bool>> _compiledPredicate;
    private readonly List<Expression<Func<T, object>>> _includes = [];
    private readonly List<string> _includeStrings = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{T}"/> class.
    /// Lazily compiles the expression for efficient repeated evaluations.
    /// </summary>
    protected Specification()
    {
        _compiledPredicate = new Lazy<Func<T, bool>>(() => ToExpression().Compile());
    }

    /// <summary>
    /// When overridden in a derived class, returns the expression representing this specification's criteria.
    /// </summary>
    /// <returns>The LINQ expression for this specification.</returns>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <inheritdoc/>
    public virtual bool IsSatisfiedBy(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return _compiledPredicate.Value(entity);
    }

    /// <inheritdoc/>
    public virtual Task<bool> IsSatisfiedByAsync(T entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(IsSatisfiedBy(entity));
    }

    /// <inheritdoc/>
    public virtual Result<bool> Evaluate(T entity)
    {
        try
        {
            return Result.Success(IsSatisfiedBy(entity));
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Specification '{GetType().Name}' evaluation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public virtual async Task<Result<bool>> EvaluateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await IsSatisfiedByAsync(entity, cancellationToken);
            return Result.Success(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Specification '{GetType().Name}' async evaluation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();

    /// <inheritdoc/>
    public IReadOnlyList<string> IncludeStrings => _includeStrings.AsReadOnly();

    /// <inheritdoc/>
    public virtual bool IgnoreQueryFilters => false;

    /// <summary>
    /// Adds an Include expression for eager loading a navigation property.
    /// </summary>
    /// <param name="includeExpression">The expression selecting the navigation property.</param>
    /// <returns>This specification for fluent chaining.</returns>
    protected Specification<T> AddInclude(Expression<Func<T, object>> includeExpression)
    {
        ArgumentNullException.ThrowIfNull(includeExpression);
        _includes.Add(includeExpression);
        return this;
    }

    /// <summary>
    /// Adds a string-based Include path for complex navigation scenarios (ThenInclude).
    /// </summary>
    /// <param name="includePath">The dot-separated path (e.g., "Members.Invitations").</param>
    /// <returns>This specification for fluent chaining.</returns>
    protected Specification<T> AddInclude(string includePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(includePath);
        _includeStrings.Add(includePath);
        return this;
    }

    /// <summary>
    /// Combines this specification with another using logical AND.
    /// Both specifications must be satisfied.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new AndSpecification combining both.</returns>
    public static Specification<T> operator &(Specification<T> left, Specification<T> right)
        => new AndSpecification<T>(left, right);

    /// <summary>
    /// Combines this specification with another using logical OR.
    /// Either specification must be satisfied.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new OrSpecification combining both.</returns>
    public static Specification<T> operator |(Specification<T> left, Specification<T> right)
        => new OrSpecification<T>(left, right);

    /// <summary>
    /// Negates this specification.
    /// </summary>
    /// <param name="spec">The specification to negate.</param>
    /// <returns>A new NotSpecification negating the original.</returns>
    public static Specification<T> operator !(Specification<T> spec)
        => new NotSpecification<T>(spec);

    /// <summary>
    /// Implicitly converts a specification to its expression.
    /// Enables direct use in LINQ Where clauses.
    /// </summary>
    /// <param name="specification">The specification to convert.</param>
    public static implicit operator Expression<Func<T, bool>>(Specification<T> specification)
        => specification.ToExpression();
}
