using System.Linq.Expressions;

namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Specification decorator that adds pagination capability to an ordered specification.
/// Implements IPaginatedSpecification by wrapping an IOrderedSpecification.
/// </summary>
/// <typeparam name="T">The entity type being queried.</typeparam>
/// <remarks>
/// Pagination requires ordering to ensure consistent, predictable results.
/// The Skip and Take values are applied after filtering and ordering.
/// </remarks>
public sealed class PaginatedSpecification<T> : IPaginatedSpecification<T>
    where T : class
{
    private readonly IOrderedSpecification<T> _baseSpecification;
    private readonly Lazy<Func<T, bool>> _compiledPredicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedSpecification{T}"/> class.
    /// </summary>
    /// <param name="baseSpecification">The ordered specification to paginate.</param>
    /// <param name="skip">Number of entities to skip (offset).</param>
    /// <param name="take">Maximum number of entities to return (limit).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when skip is negative or take is not positive.
    /// </exception>
    public PaginatedSpecification(IOrderedSpecification<T> baseSpecification, int skip, int take)
    {
        _baseSpecification = baseSpecification ?? throw new ArgumentNullException(nameof(baseSpecification));

        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip), "Skip must be non-negative.");
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be positive.");
        }

        Skip = skip;
        Take = take;
        _compiledPredicate = new Lazy<Func<T, bool>>(() => ToExpression().Compile());
    }

    /// <inheritdoc/>
    public int Skip { get; }

    /// <inheritdoc/>
    public int Take { get; }

    /// <inheritdoc/>
    public IReadOnlyList<OrderExpression<T>> OrderExpressions => _baseSpecification.OrderExpressions;

    /// <inheritdoc/>
    public Expression<Func<T, bool>> ToExpression() => _baseSpecification.ToExpression();

    /// <inheritdoc/>
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _baseSpecification.Includes;

    /// <inheritdoc/>
    public IReadOnlyList<string> IncludeStrings => _baseSpecification.IncludeStrings;

    /// <inheritdoc/>
    public bool IgnoreQueryFilters => _baseSpecification.IgnoreQueryFilters;

    /// <inheritdoc/>
    public bool IsSatisfiedBy(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return _compiledPredicate.Value(entity);
    }

    /// <inheritdoc/>
    public Task<bool> IsSatisfiedByAsync(T entity, CancellationToken cancellationToken = default)
        => _baseSpecification.IsSatisfiedByAsync(entity, cancellationToken);

    /// <inheritdoc/>
    public Result<bool> Evaluate(T entity)
        => _baseSpecification.Evaluate(entity);

    /// <inheritdoc/>
    public Task<Result<bool>> EvaluateAsync(T entity, CancellationToken cancellationToken = default)
        => _baseSpecification.EvaluateAsync(entity, cancellationToken);

    /// <summary>
    /// Creates a new PaginatedSpecification for a different page.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A new PaginatedSpecification for the specified page.</returns>
    public PaginatedSpecification<T> ForPage(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be at least 1.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be positive.");
        }

        var skip = (pageNumber - 1) * pageSize;
        return new PaginatedSpecification<T>(_baseSpecification, skip, pageSize);
    }
}
