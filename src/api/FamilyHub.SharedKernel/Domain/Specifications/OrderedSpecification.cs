using System.Linq.Expressions;

namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Specification decorator that adds ordering capability to an existing specification.
/// Implements IOrderedSpecification by wrapping a base IQueryableSpecification.
/// </summary>
/// <typeparam name="T">The entity type being queried.</typeparam>
public sealed class OrderedSpecification<T> : IOrderedSpecification<T>
    where T : class
{
    private readonly IQueryableSpecification<T> _baseSpecification;
    private readonly List<OrderExpression<T>> _orderExpressions;
    private readonly Lazy<Func<T, bool>> _compiledPredicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderedSpecification{T}"/> class.
    /// </summary>
    /// <param name="baseSpecification">The base specification to decorate.</param>
    /// <param name="keySelector">The initial order key selector.</param>
    /// <param name="isDescending">Whether to order descending.</param>
    public OrderedSpecification(
        IQueryableSpecification<T> baseSpecification,
        Expression<Func<T, object>> keySelector,
        bool isDescending = false)
    {
        _baseSpecification = baseSpecification ?? throw new ArgumentNullException(nameof(baseSpecification));
        ArgumentNullException.ThrowIfNull(keySelector);

        _orderExpressions = [new OrderExpression<T>(keySelector, isDescending)];
        _compiledPredicate = new Lazy<Func<T, bool>>(() => ToExpression().Compile());
    }

    /// <summary>
    /// Initializes a new instance with existing order expressions (for chaining).
    /// </summary>
    private OrderedSpecification(
        IQueryableSpecification<T> baseSpecification,
        List<OrderExpression<T>> orderExpressions)
    {
        _baseSpecification = baseSpecification;
        _orderExpressions = orderExpressions;
        _compiledPredicate = new Lazy<Func<T, bool>>(() => ToExpression().Compile());
    }

    /// <inheritdoc/>
    public IReadOnlyList<OrderExpression<T>> OrderExpressions => _orderExpressions.AsReadOnly();

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
    /// Adds an additional ascending order expression (ThenBy).
    /// </summary>
    /// <param name="keySelector">The secondary order key selector.</param>
    /// <returns>A new OrderedSpecification with the additional ordering.</returns>
    public OrderedSpecification<T> ThenBy(Expression<Func<T, object>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        var newOrderings = new List<OrderExpression<T>>(_orderExpressions)
        {
            new(keySelector, isDescending: false)
        };
        return new OrderedSpecification<T>(_baseSpecification, newOrderings);
    }

    /// <summary>
    /// Adds an additional descending order expression (ThenByDescending).
    /// </summary>
    /// <param name="keySelector">The secondary order key selector.</param>
    /// <returns>A new OrderedSpecification with the additional ordering.</returns>
    public OrderedSpecification<T> ThenByDescending(Expression<Func<T, object>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        var newOrderings = new List<OrderExpression<T>>(_orderExpressions)
        {
            new(keySelector, isDescending: true)
        };
        return new OrderedSpecification<T>(_baseSpecification, newOrderings);
    }
}
