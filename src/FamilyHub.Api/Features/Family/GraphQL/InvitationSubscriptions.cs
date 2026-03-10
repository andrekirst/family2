using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;
using HotChocolate.Resolvers;

namespace FamilyHub.Api.Features.Family.GraphQL;

/// <summary>
/// GraphQL subscriptions for real-time invitation updates.
/// Topic is family-scoped: "InvitationUpdated_{familyId}" ensures clients only receive
/// invitation changes for their own family.
///
/// Security note: The [Authorize] attribute ensures authentication. The topic includes
/// the familyId so clients only receive events for the family they subscribe to.
/// For defense-in-depth, a WebSocket interceptor (ISocketSessionInterceptor) should
/// validate family membership on connection init to prevent a malicious client from
/// subscribing to another family's topic.
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
        [EventMessage] InvitationDto invitation,
        IResolverContext context)
    {
        // TODO: Validate that the subscribing user belongs to the requested family.
        // This requires either:
        // 1. A WebSocket interceptor (ISocketSessionInterceptor) that sets GlobalState
        //    with the user's familyId on connection init, or
        // 2. Resolving the user's family membership from claims/database here.
        // Currently, the [Authorize] attribute ensures authentication and the family-scoped
        // topic provides partial isolation, but a determined attacker could subscribe to
        // another family's topic if they know the familyId GUID.
        return invitation;
    }
}
