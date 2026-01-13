using System.Linq.Expressions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.Specifications;

namespace FamilyHub.Modules.Family.Domain.Specifications;

/// <summary>
/// Specification for expired invitations ready for cleanup.
/// Matches invitations that are expired and past the expiration threshold.
/// Replaces: GetExpiredInvitationsForCleanupAsync(DateTime)
/// </summary>
/// <param name="expirationThreshold">Invitations expired before this time are included.</param>
public sealed class ExpiredInvitationForCleanupSpecification(DateTime expirationThreshold)
    : Specification<FamilyMemberInvitation>
{
    /// <inheritdoc/>
    public override Expression<Func<FamilyMemberInvitation, bool>> ToExpression()
        => invitation => invitation.ExpiresAt < expirationThreshold
                      && invitation.Status == InvitationStatus.Expired;
}
