using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.RevokeInvitation;

[ExtendObjectType(typeof(FamilyInvitationMutation))]
public class MutationType
{
    /// <summary>
    /// Revoke a pending family invitation (Owner/Admin only).
    /// </summary>
    [Authorize]
    public async Task<bool> Revoke(
        [Parent] FamilyInvitationMutation parent,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var invitationId = parent.InvitationId.HasValue
            ? InvitationId.From(parent.InvitationId.Value)
            : (InvitationId?)null;

        var command = new RevokeInvitationCommand(invitationId);
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
