using System.Security.Claims;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RevokeShareLink;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<RevokeShareLinkResult> RevokeShareLink(
        Guid shareLinkId,
        Guid familyId,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var user = await userRepository.GetByExternalIdAsync(
            ExternalUserId.From(externalUserIdString), cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found");

        var command = new RevokeShareLinkCommand(
            ShareLinkId.From(shareLinkId),
            FamilyId.From(familyId),
            user.Id);

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
