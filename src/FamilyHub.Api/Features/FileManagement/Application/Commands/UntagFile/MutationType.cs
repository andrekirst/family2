using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UntagFile;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<bool> UntagFile(
        Guid fileId,
        Guid tagId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new UntagFileCommand(FileId.From(fileId), TagId.From(tagId));
        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Success;
    }
}
