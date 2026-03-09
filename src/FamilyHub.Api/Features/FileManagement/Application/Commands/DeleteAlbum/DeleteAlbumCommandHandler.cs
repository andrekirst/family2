using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteAlbum;

public sealed class DeleteAlbumCommandHandler(
    IAlbumRepository albumRepository,
    IAlbumItemRepository albumItemRepository)
    : ICommandHandler<DeleteAlbumCommand, Result<DeleteAlbumResult>>
{
    public async ValueTask<Result<DeleteAlbumResult>> Handle(
        DeleteAlbumCommand command,
        CancellationToken cancellationToken)
    {
        var album = await albumRepository.GetByIdAsync(command.AlbumId, cancellationToken);
        if (album is null)
        {
            return DomainError.NotFound(DomainErrorCodes.AlbumNotFound, "Album not found");
        }

        if (album.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Album belongs to a different family");
        }

        await albumItemRepository.RemoveByAlbumIdAsync(command.AlbumId, cancellationToken);

        await albumRepository.RemoveAsync(album, cancellationToken);

        return new DeleteAlbumResult(true);
    }
}
