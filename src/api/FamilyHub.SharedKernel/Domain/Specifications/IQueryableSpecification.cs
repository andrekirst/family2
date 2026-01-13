using System.Linq.Expressions;

namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Specification interface that supports EF Core IQueryable operations.
/// Extends ISpecification with expression-based filtering for database queries.
/// </summary>
/// <typeparam name="T">The entity type to query. Must be a reference type for EF Core compatibility.</typeparam>
public interface IQueryableSpecification<T> : ISpecification<T>
    where T : class
{
    /// <summary>
    /// Gets the LINQ expression representing this specification's criteria.
    /// This expression is translated to SQL by EF Core.
    /// </summary>
    /// <returns>An expression that can be used in LINQ Where clauses.</returns>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Gets the collection of Include expressions for eager loading navigation properties.
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the collection of string-based Include paths for ThenInclude scenarios.
    /// Use this for deep navigation property chains (e.g., "Members.Invitations").
    /// </summary>
    IReadOnlyList<string> IncludeStrings { get; }

    /// <summary>
    /// Gets a value indicating whether to ignore EF Core global query filters.
    /// When true, soft-deleted entities will be included in query results.
    /// Use sparingly - typically only for admin or cleanup operations.
    /// </summary>
    bool IgnoreQueryFilters { get; }
}
