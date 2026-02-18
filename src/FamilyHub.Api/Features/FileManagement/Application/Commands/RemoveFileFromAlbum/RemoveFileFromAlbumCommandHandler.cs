using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RemoveFileFromAlbum;

public sealed class RemoveFileFromAlbumCommandHandler(
    IAlbumRepository albumRepository,
    IAlbumItemRepository albumItemRepository)
    : ICommandHandler<RemoveFileFromAlbumCommand, RemoveFileFromAlbumResult>
{
    public async ValueTask<RemoveFileFromAlbumResult> Handle(
        RemoveFileFromAlbumCommand command,
        CancellationToken cancellationToken)
    {
        var album = await albumRepository.GetByIdAsync(command.AlbumId, cancellationToken)
            ?? throw new DomainException("Album not found", DomainErrorCodes.AlbumNotFound);

        if (album.FamilyId != command.FamilyId)
            throw new DomainException("Album belongs to a different family", DomainErrorCodes.Forbidden);

        var items = await albumItemRepository.GetByAlbumIdAsync(command.AlbumId, cancellationToken);
        var item = items.FirstOrDefault(ai => ai.FileId == command.FileId);

        if (item is null)
            return new RemoveFileFromAlbumResult(true); // Idempotent

        await albumItemRepository.RemoveAsync(item, cancellationToken);

        // If cover was removed, auto-select new cover
        if (album.CoverFileId == command.FileId)
        {
            var firstFileId = await albumItemRepository.GetFirstImageFileIdAsync(command.AlbumId, cancellationToken);
            album.SetCoverImage(firstFileId);
        }

        return new RemoveFileFromAlbumResult(true);
    }
}
