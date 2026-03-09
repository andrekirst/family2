using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DisconnectExternalStorage;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<object> DisconnectExternalStorage(
        Guid connectionId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new DisconnectExternalStorageCommand(
            ExternalConnectionId.From(connectionId));

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match<object>(
            success => success,
            error => MutationError.FromDomainError(error));
    }
}
