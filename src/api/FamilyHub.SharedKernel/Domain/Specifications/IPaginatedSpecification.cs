namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Specification interface that supports pagination.
/// Extends IOrderedSpecification with skip/take parameters.
/// </summary>
/// <typeparam name="T">The entity type to query with pagination.</typeparam>
/// <remarks>
/// Pagination requires ordering to ensure consistent results.
/// If no explicit ordering is provided, ensure a default order exists.
/// </remarks>
public interface IPaginatedSpecification<T> : IOrderedSpecification<T>
    where T : class
{
    /// <summary>
    /// Gets the number of entities to skip (offset).
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Gets the maximum number of entities to return (limit).
    /// </summary>
    int Take { get; }
}
