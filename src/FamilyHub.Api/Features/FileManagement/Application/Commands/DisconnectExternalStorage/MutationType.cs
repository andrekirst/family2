using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DisconnectExternalStorage;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<DisconnectExternalStorageResult> DisconnectExternalStorage(
        Guid connectionId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new DisconnectExternalStorageCommand(
            ExternalConnectionId.From(connectionId));

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
