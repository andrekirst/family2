using System.Linq.Expressions;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Specifications;

/// <summary>
/// Specification for finding active users in a specific family.
/// Replaces: GetByFamilyIdAsync(FamilyId)
/// </summary>
/// <param name="familyId">The family ID to filter by.</param>
public sealed class UsersByFamilySpecification(FamilyId familyId) : Specification<User>
{
    /// <inheritdoc/>
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.FamilyId == familyId && user.DeletedAt == null;
}
