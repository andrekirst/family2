using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.AddFileToAlbum;

public sealed class AddFileToAlbumCommandHandler(
    IAlbumRepository albumRepository,
    IStoredFileRepository storedFileRepository,
    IAlbumItemRepository albumItemRepository)
    : ICommandHandler<AddFileToAlbumCommand, AddFileToAlbumResult>
{
    public async ValueTask<AddFileToAlbumResult> Handle(
        AddFileToAlbumCommand command,
        CancellationToken cancellationToken)
    {
        var album = await albumRepository.GetByIdAsync(command.AlbumId, cancellationToken)
            ?? throw new DomainException("Album not found", DomainErrorCodes.AlbumNotFound);

        if (album.FamilyId != command.FamilyId)
            throw new DomainException("Album belongs to a different family", DomainErrorCodes.Forbidden);

        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.FileNotFound);

        if (file.FamilyId != command.FamilyId)
            throw new DomainException("File belongs to a different family", DomainErrorCodes.Forbidden);

        // Idempotent: if already in album, return success
        var exists = await albumItemRepository.ExistsAsync(command.AlbumId, command.FileId, cancellationToken);
        if (exists)
            return new AddFileToAlbumResult(true);

        var item = AlbumItem.Create(command.AlbumId, command.FileId, command.AddedBy);
        await albumItemRepository.AddAsync(item, cancellationToken);

        // Auto-set cover image if not set
        if (album.CoverFileId is null)
            album.SetCoverImage(command.FileId);

        return new AddFileToAlbumResult(true);
    }
}
