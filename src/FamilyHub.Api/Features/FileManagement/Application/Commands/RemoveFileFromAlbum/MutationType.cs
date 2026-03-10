using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RemoveFileFromAlbum;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<bool> RemoveFileFromAlbum(
        Guid albumId,
        Guid fileId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new RemoveFileFromAlbumCommand(
            AlbumId.From(albumId),
            FileId.From(fileId));

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
