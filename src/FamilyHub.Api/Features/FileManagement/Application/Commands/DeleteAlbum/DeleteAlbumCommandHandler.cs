using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteAlbum;

public sealed class DeleteAlbumCommandHandler(
    IAlbumRepository albumRepository,
    IAlbumItemRepository albumItemRepository)
    : ICommandHandler<DeleteAlbumCommand, DeleteAlbumResult>
{
    public async ValueTask<DeleteAlbumResult> Handle(
        DeleteAlbumCommand command,
        CancellationToken cancellationToken)
    {
        var album = await albumRepository.GetByIdAsync(command.AlbumId, cancellationToken)
            ?? throw new DomainException("Album not found", DomainErrorCodes.AlbumNotFound);

        if (album.FamilyId != command.FamilyId)
            throw new DomainException("Album belongs to a different family", DomainErrorCodes.Forbidden);

        // Remove all album items first
        await albumItemRepository.RemoveByAlbumIdAsync(command.AlbumId, cancellationToken);

        // Remove the album
        await albumRepository.RemoveAsync(album, cancellationToken);

        return new DeleteAlbumResult(true);
    }
}
