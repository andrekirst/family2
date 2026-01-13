using System.Linq.Expressions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Specifications;

/// <summary>
/// Specification for pending invitations to a specific email in a family.
/// Replaces: GetPendingByEmailAsync(FamilyId, Email)
/// </summary>
/// <param name="familyId">The family ID to filter by.</param>
/// <param name="email">The email address to filter by.</param>
public sealed class PendingInvitationByEmailSpecification(FamilyId familyId, Email email)
    : Specification<FamilyMemberInvitation>
{
    /// <inheritdoc/>
    public override Expression<Func<FamilyMemberInvitation, bool>> ToExpression()
        => invitation => invitation.FamilyId == familyId
                      && invitation.Email == email
                      && invitation.Status == InvitationStatus.Pending;
}
