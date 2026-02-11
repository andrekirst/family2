using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitation;

[ExtendObjectType(typeof(FamilyInvitationMutation))]
public class MutationType
{
    /// <summary>
    /// Accept a family invitation using the token from the email link.
    /// </summary>
    [Authorize]
    public async Task<AcceptInvitationResultDto> AcceptByToken(
        AcceptInvitationRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        var command = new AcceptInvitationCommand(input.Token, user.Id);
        var result = await commandBus.SendAsync(command, cancellationToken);

        return new AcceptInvitationResultDto
        {
            FamilyId = result.FamilyId.Value,
            FamilyMemberId = result.FamilyMemberId.Value,
            Success = true
        };
    }
}
