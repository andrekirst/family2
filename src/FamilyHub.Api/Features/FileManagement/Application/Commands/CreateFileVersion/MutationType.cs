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
    public async Task<bool> CreateFileVersion(
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

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match(
            success => true,
            error => throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Message)
                    .SetCode(error.ErrorCode)
                    .Build()));
    }
}
