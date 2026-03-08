using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.SetFamilyAvatar;

[ExtendObjectType(typeof(FamilyMutation))]
public class MutationType
{
    /// <summary>
    /// Set a per-family avatar override for the current user.
    /// </summary>
    [Authorize]
    public async Task<bool> SetFamilyAvatar(
        SetFamilyAvatarInput input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new SetFamilyAvatarCommand(
            AvatarId.From(input.AvatarId));

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Success;
    }
}

public record SetFamilyAvatarInput(Guid AvatarId);
