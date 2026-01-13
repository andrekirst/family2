namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Specification interface that supports ordering.
/// Extends IQueryableSpecification with ordering expressions.
/// </summary>
/// <typeparam name="T">The entity type to query and order.</typeparam>
public interface IOrderedSpecification<T> : IQueryableSpecification<T>
    where T : class
{
    /// <summary>
    /// Gets the collection of order expressions defining the sort order.
    /// Multiple expressions enable multi-level sorting (ORDER BY x, y, z).
    /// </summary>
    IReadOnlyList<OrderExpression<T>> OrderExpressions { get; }
}
