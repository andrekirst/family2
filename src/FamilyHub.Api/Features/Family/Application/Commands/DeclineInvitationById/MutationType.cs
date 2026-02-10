using System.Security.Claims;
using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
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
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var invitationId = parent.InvitationId
            ?? throw new GraphQLException("Invitation ID required. Use invitation(id: \"...\") { decline }");

        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        var command = new DeclineInvitationByIdCommand(InvitationId.From(invitationId), user.Id);
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
