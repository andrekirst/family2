using System.Linq.Expressions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.Specifications;

namespace FamilyHub.Modules.Family.Domain.Specifications;

/// <summary>
/// Specification for pending invitations.
/// Matches invitations with Status = Pending.
/// </summary>
public sealed class PendingInvitationSpecification : Specification<FamilyMemberInvitation>
{
    /// <inheritdoc/>
    public override Expression<Func<FamilyMemberInvitation, bool>> ToExpression()
        => invitation => invitation.Status == InvitationStatus.Pending;
}
