using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
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
        var command = new SetPermissionCommand(
            input.ResourceType,
            input.ResourceId,
            UserId.From(input.MemberId),
            input.PermissionLevel);

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
