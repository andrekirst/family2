using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
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
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var invitationId = parent.InvitationId
            ?? throw new GraphQLException("Invitation ID required. Use invitation(id: \"...\") { revoke }");

        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException("You must be part of a family to revoke invitations");
        }

        var command = new RevokeInvitationCommand(InvitationId.From(invitationId), user.Id, user.FamilyId.Value);
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
