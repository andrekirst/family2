using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.GraphQL;

/// <summary>
/// GraphQL subscriptions for real-time invitation updates.
/// Topic is family-scoped: "InvitationUpdated_{familyId}" ensures clients only receive
/// invitation changes for their own family.
/// </summary>
[ExtendObjectType("Subscription")]
public class InvitationSubscriptions
{
    /// <summary>
    /// Subscribe to invitation status changes (sent, accepted, declined, revoked) for a family.
    /// </summary>
    [Authorize]
    [Subscribe]
    [Topic("InvitationUpdated_{familyId}")]
    public InvitationDto InvitationUpdated(
        Guid familyId,
        [EventMessage] InvitationDto invitation)
        => invitation;
}
