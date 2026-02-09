using System.Security.Claims;
using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.GraphQL;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitationById;

[ExtendObjectType(typeof(AuthMutations))]
public class MutationType
{
    /// <summary>
    /// Accept a family invitation by ID (from the dashboard, without the email token).
    /// Verifies that the accepting user's email matches the invitation's invitee email.
    /// </summary>
    [Authorize]
    public async Task<AcceptInvitationResultDto> AcceptInvitationById(
        Guid invitationId,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
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
