using System.Linq.Expressions;

namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Represents an ordering expression for specifications.
/// Used by IOrderedSpecification to define sort order.
/// </summary>
/// <typeparam name="T">The entity type being ordered.</typeparam>
public sealed class OrderExpression<T>
{
    /// <summary>
    /// Gets the expression that selects the property to order by.
    /// </summary>
    public Expression<Func<T, object>> KeySelector { get; }

    /// <summary>
    /// Gets a value indicating whether to order in descending direction.
    /// </summary>
    public bool IsDescending { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderExpression{T}"/> class.
    /// </summary>
    /// <param name="keySelector">The expression selecting the order key.</param>
    /// <param name="isDescending">True for descending order; false for ascending.</param>
    public OrderExpression(Expression<Func<T, object>> keySelector, bool isDescending = false)
    {
        KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        IsDescending = isDescending;
    }

    /// <summary>
    /// Creates an ascending order expression.
    /// </summary>
    /// <param name="keySelector">The expression selecting the order key.</param>
    /// <returns>A new ascending OrderExpression.</returns>
    public static OrderExpression<T> Ascending(Expression<Func<T, object>> keySelector)
        => new(keySelector, isDescending: false);

    /// <summary>
    /// Creates a descending order expression.
    /// </summary>
    /// <param name="keySelector">The expression selecting the order key.</param>
    /// <returns>A new descending OrderExpression.</returns>
    public static OrderExpression<T> Descending(Expression<Func<T, object>> keySelector)
        => new(keySelector, isDescending: true);
}
