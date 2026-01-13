using System.Linq.Expressions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Specifications;

/// <summary>
/// Specification for pending invitations in a specific family.
/// Replaces: GetPendingByFamilyIdAsync(FamilyId)
/// </summary>
/// <param name="familyId">The family ID to filter by.</param>
public sealed class PendingInvitationByFamilySpecification(FamilyId familyId) : Specification<FamilyMemberInvitation>
{
    /// <inheritdoc/>
    public override Expression<Func<FamilyMemberInvitation, bool>> ToExpression()
        => invitation => invitation.FamilyId == familyId
                      && invitation.Status == InvitationStatus.Pending;
}
