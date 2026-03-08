using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.UnlinkGoogleAccount;

[ExtendObjectType(typeof(GoogleIntegrationMutation))]
public class MutationType
{
    [Authorize]
    public async Task<bool> Unlink(
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new UnlinkGoogleAccountCommand();
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
