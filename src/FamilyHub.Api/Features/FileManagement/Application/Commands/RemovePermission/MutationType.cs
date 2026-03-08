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
        PermissionResourceType resourceType,
        Guid resourceId,
        Guid memberId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new RemovePermissionCommand(
            resourceType,
            resourceId,
            UserId.From(memberId));

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
