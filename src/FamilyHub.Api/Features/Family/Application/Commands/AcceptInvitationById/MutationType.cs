using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitationById;

[ExtendObjectType(typeof(FamilyInvitationMutation))]
public class MutationType
{
    /// <summary>
    /// Accept a family invitation by ID (from the dashboard, without the email token).
    /// Verifies that the accepting user's email matches the invitation's invitee email.
    /// </summary>
    [Authorize]
    public async Task<AcceptInvitationResultDto> Accept(
        [Parent] FamilyInvitationMutation parent,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var invitationId = parent.InvitationId
            ?? throw new GraphQLException("Invitation ID required. Use invitation(id: \"...\") { accept }");

        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        var command = new AcceptInvitationByIdCommand(InvitationId.From(invitationId), user.Id);
        var result = await commandBus.SendAsync(command, cancellationToken);

        return new AcceptInvitationResultDto
        {
            FamilyId = result.FamilyId.Value,
            FamilyMemberId = result.FamilyMemberId.Value,
            Success = true
        };
    }
}
