using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateShareLink;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<CreateShareLinkResult> CreateShareLink(
        string resourceType,
        Guid resourceId,
        DateTime? expiresAt,
        string? password,
        int? maxDownloads,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var parsedResourceType = Enum.Parse<ShareResourceType>(resourceType, ignoreCase: true);

        var command = new CreateShareLinkCommand(
            parsedResourceType,
            resourceId,
            expiresAt,
            password,
            maxDownloads);

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
