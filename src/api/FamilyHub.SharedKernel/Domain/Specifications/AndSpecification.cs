using System.Linq.Expressions;

namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Composite specification that combines two specifications with logical AND.
/// Both specifications must be satisfied for the entity to match.
/// </summary>
/// <typeparam name="T">The entity type being evaluated.</typeparam>
/// <param name="left">The left specification.</param>
/// <param name="right">The right specification.</param>
public sealed class AndSpecification<T>(IQueryableSpecification<T> left, IQueryableSpecification<T> right)
    : Specification<T>
    where T : class
{
    /// <inheritdoc/>
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();

        var parameter = Expression.Parameter(typeof(T), "x");

        var combined = Expression.AndAlso(
            Expression.Invoke(leftExpr, parameter),
            Expression.Invoke(rightExpr, parameter));

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    /// <inheritdoc/>
    public override bool IgnoreQueryFilters => left.IgnoreQueryFilters || right.IgnoreQueryFilters;

    /// <inheritdoc/>
    public override async Task<bool> IsSatisfiedByAsync(T entity, CancellationToken cancellationToken = default)
    {
        var leftResult = await left.IsSatisfiedByAsync(entity, cancellationToken);
        if (!leftResult)
        {
            return false; // Short-circuit: no need to evaluate right if left is false
        }

        return await right.IsSatisfiedByAsync(entity, cancellationToken);
    }
}
