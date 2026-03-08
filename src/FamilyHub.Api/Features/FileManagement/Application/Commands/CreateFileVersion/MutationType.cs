using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFileVersion;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<CreateFileVersionResult> CreateFileVersion(
        Guid fileId,
        string storageKey,
        long fileSize,
        string checksum,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new CreateFileVersionCommand(
            FileId.From(fileId),
            StorageKey.From(storageKey),
            FileSize.From(fileSize),
            Checksum.From(checksum));

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
