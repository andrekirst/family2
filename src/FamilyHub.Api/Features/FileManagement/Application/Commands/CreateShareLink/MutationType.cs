using System.Security.Claims;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateShareLink;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<CreateShareLinkResult> CreateShareLink(
        string resourceType,
        Guid resourceId,
        Guid familyId,
        DateTime? expiresAt,
        string? password,
        int? maxDownloads,
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

        var parsedResourceType = Enum.Parse<ShareResourceType>(resourceType, ignoreCase: true);

        var command = new CreateShareLinkCommand(
            parsedResourceType,
            resourceId,
            FamilyId.From(familyId),
            user.Id,
            expiresAt,
            password,
            maxDownloads);

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
