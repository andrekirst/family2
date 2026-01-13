using System.Linq.Expressions;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Specifications;

/// <summary>
/// Specification for finding a family by its ID.
/// Replaces: GetByIdAsync(FamilyId)
/// </summary>
/// <param name="id">The family ID to search for.</param>
public sealed class FamilyByIdSpecification(FamilyId id) : Specification<Aggregates.Family>
{
    /// <inheritdoc/>
    public override Expression<Func<Aggregates.Family, bool>> ToExpression()
        => family => family.Id == id;
}
