using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.SetPermission;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<SetPermissionResult> SetPermission(
        SetPermissionRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var resourceType = input.ResourceType.ToLowerInvariant() switch
        {
            "file" => PermissionResourceType.File,
            "folder" => PermissionResourceType.Folder,
            _ => throw new ArgumentException($"Invalid resource type: {input.ResourceType}")
        };

        var permissionLevel = (FilePermissionLevel)input.PermissionLevel;
        if (!Enum.IsDefined(permissionLevel))
        {
            throw new ArgumentException($"Invalid permission level: {input.PermissionLevel}");
        }

        var command = new SetPermissionCommand(
            resourceType,
            input.ResourceId,
            UserId.From(input.MemberId),
            permissionLevel);

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
