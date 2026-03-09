using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFolder;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<object> DeleteFolder(
        Guid folderId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFolderCommand(
            FolderId.From(folderId));

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match<object>(
            success => success,
            error => MutationError.FromDomainError(error));
    }
}
