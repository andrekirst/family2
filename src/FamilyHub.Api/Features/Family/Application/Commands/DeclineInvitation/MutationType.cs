using System.Security.Claims;
using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.GraphQL;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.DeclineInvitation;

[ExtendObjectType(typeof(AuthMutations))]
public class MutationType
{
    /// <summary>
    /// Decline a family invitation using the token from the email link.
    /// </summary>
    [Authorize]
    public async Task<bool> DeclineInvitation(
        AcceptInvitationRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        // Verify the user is authenticated (even though we don't need user data for decline)
        await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        var command = new DeclineInvitationCommand(input.Token);
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
