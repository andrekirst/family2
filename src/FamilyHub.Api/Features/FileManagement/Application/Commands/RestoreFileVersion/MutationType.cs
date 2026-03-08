using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RestoreFileVersion;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<RestoreFileVersionResult> RestoreFileVersion(
        Guid versionId,
        Guid fileId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new RestoreFileVersionCommand(
            FileVersionId.From(versionId),
            FileId.From(fileId));

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
