using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.RemoveAvatar;

[ExtendObjectType(typeof(FamilyMutation))]
public class MutationType
{
    /// <summary>
    /// Remove the current user's global avatar.
    /// </summary>
    [Authorize]
    public async Task<bool> RemoveAvatar(
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new RemoveAvatarCommand();
        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.Success;
    }
}
