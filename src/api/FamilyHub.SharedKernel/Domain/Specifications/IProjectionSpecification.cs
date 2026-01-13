using System.Linq.Expressions;

namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Specification interface that supports projection to a different type.
/// Enables selecting a subset of entity properties into a DTO or view model.
/// </summary>
/// <typeparam name="T">The source entity type.</typeparam>
/// <typeparam name="TResult">The projected result type.</typeparam>
/// <remarks>
/// Projections are applied after filtering to optimize database queries.
/// EF Core translates the Selector expression to a SQL SELECT clause.
/// </remarks>
public interface IProjectionSpecification<T, TResult> : IQueryableSpecification<T>
    where T : class
{
    /// <summary>
    /// Gets the expression that projects the entity to the result type.
    /// </summary>
    Expression<Func<T, TResult>> Selector { get; }
}
