using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ProcessInboxFiles;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<ProcessInboxFilesResult> ProcessInboxFiles(
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new ProcessInboxFilesCommand();

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
