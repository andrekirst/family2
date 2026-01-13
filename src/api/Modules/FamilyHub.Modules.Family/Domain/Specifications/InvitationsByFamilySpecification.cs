using System.Linq.Expressions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Specifications;

/// <summary>
/// Specification for all invitations (any status) in a specific family.
/// Replaces: GetByFamilyIdAsync(FamilyId)
/// </summary>
/// <param name="familyId">The family ID to filter by.</param>
public sealed class InvitationsByFamilySpecification(FamilyId familyId) : Specification<FamilyMemberInvitation>
{
    /// <inheritdoc/>
    public override Expression<Func<FamilyMemberInvitation, bool>> ToExpression()
        => invitation => invitation.FamilyId == familyId;
}
