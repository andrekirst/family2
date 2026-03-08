using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.DeclineInvitationById;

[ExtendObjectType(typeof(FamilyInvitationMutation))]
public class MutationType
{
    /// <summary>
    /// Decline a family invitation by ID (from the dashboard, without the email token).
    /// Verifies that the declining user's email matches the invitation's invitee email.
    /// </summary>
    [Authorize]
    public async Task<bool> Decline(
        [Parent] FamilyInvitationMutation parent,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var invitationId = parent.InvitationId.HasValue
            ? InvitationId.From(parent.InvitationId.Value)
            : (InvitationId?)null;

        var command = new DeclineInvitationByIdCommand(invitationId);
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
