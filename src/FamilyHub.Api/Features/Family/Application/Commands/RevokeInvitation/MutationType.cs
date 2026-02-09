using System.Security.Claims;
using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.GraphQL;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.RevokeInvitation;

[ExtendObjectType(typeof(AuthMutations))]
public class MutationType
{
    /// <summary>
    /// Revoke a pending family invitation (Owner/Admin only).
    /// </summary>
    [Authorize]
    public async Task<bool> RevokeInvitation(
        Guid invitationId,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        var command = new RevokeInvitationCommand(InvitationId.From(invitationId), user.Id);
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
