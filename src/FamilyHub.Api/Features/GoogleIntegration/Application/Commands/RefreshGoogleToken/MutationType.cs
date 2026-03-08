using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.GoogleIntegration.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.RefreshGoogleToken;

[ExtendObjectType(typeof(GoogleIntegrationMutation))]
public class MutationType
{
    [Authorize]
    public async Task<RefreshTokenResultDto> RefreshToken(
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new RefreshGoogleTokenCommand();
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
