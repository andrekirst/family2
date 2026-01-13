using System.Linq.Expressions;
using FamilyHub.SharedKernel.Domain.Specifications;

namespace FamilyHub.Modules.Family.Domain.Specifications;

/// <summary>
/// Specification for active (non-deleted) families.
/// </summary>
public sealed class ActiveFamilySpecification : Specification<Aggregates.Family>
{
    /// <inheritdoc/>
    public override Expression<Func<Aggregates.Family, bool>> ToExpression()
        => family => family.DeletedAt == null;
}
