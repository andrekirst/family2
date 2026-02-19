using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.RefreshGoogleToken;

[ExtendObjectType(typeof(GoogleIntegrationMutation))]
public class MutationType
{
    [Authorize]
    public async Task<RefreshTokenResultDto> RefreshToken(
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(
            claimsPrincipal, userRepository, cancellationToken);

        var command = new RefreshGoogleTokenCommand(user.Id);
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
