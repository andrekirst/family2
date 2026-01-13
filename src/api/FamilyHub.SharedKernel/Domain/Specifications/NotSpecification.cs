using System.Linq.Expressions;

namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Composite specification that negates another specification.
/// The entity matches if it does NOT satisfy the inner specification.
/// </summary>
/// <typeparam name="T">The entity type being evaluated.</typeparam>
/// <param name="specification">The specification to negate.</param>
public sealed class NotSpecification<T>(IQueryableSpecification<T> specification) : Specification<T>
    where T : class
{
    /// <inheritdoc/>
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expr = specification.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "x");

        var negated = Expression.Not(Expression.Invoke(expr, parameter));

        return Expression.Lambda<Func<T, bool>>(negated, parameter);
    }

    /// <inheritdoc/>
    public override bool IgnoreQueryFilters => specification.IgnoreQueryFilters;

    /// <inheritdoc/>
    public override async Task<bool> IsSatisfiedByAsync(T entity, CancellationToken cancellationToken = default)
    {
        var result = await specification.IsSatisfiedByAsync(entity, cancellationToken);
        return !result;
    }
}
