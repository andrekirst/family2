using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RemoveFileFromAlbum;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<RemoveFileFromAlbumResult> RemoveFileFromAlbum(
        Guid albumId,
        Guid fileId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new RemoveFileFromAlbumCommand(
            AlbumId.From(albumId),
            FileId.From(fileId));

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
