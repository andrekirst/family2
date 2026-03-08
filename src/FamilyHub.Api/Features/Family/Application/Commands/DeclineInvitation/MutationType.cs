using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.DeclineInvitation;

[ExtendObjectType(typeof(FamilyInvitationMutation))]
public class MutationType
{
    /// <summary>
    /// Decline a family invitation using the token from the email link.
    /// </summary>
    [Authorize]
    public async Task<bool> DeclineByToken(
        AcceptInvitationRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new DeclineInvitationCommand(input.Token);
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
