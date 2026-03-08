using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RemovePermission;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<RemovePermissionResult> RemovePermission(
        string resourceType,
        Guid resourceId,
        Guid memberId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var parsedResourceType = resourceType.ToLowerInvariant() switch
        {
            "file" => PermissionResourceType.File,
            "folder" => PermissionResourceType.Folder,
            _ => throw new ArgumentException($"Invalid resource type: {resourceType}")
        };

        var command = new RemovePermissionCommand(
            parsedResourceType,
            resourceId,
            UserId.From(memberId));

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
