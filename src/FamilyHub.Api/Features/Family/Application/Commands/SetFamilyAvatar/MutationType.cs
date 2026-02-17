using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
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
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
                                   ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var user = await userRepository.GetByExternalIdAsync(externalUserId, cancellationToken)
                   ?? throw new UnauthorizedAccessException("User not found");

        if (!user.FamilyId.HasValue)
        {
            throw new InvalidOperationException("User is not in a family");
        }

        var command = new SetFamilyAvatarCommand(
            user.Id,
            user.FamilyId.Value,
            AvatarId.From(input.AvatarId));

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Success;
    }
}

public record SetFamilyAvatarInput(Guid AvatarId);
