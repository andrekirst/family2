using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.AddFileToAlbum;

public sealed class AddFileToAlbumCommandHandler(
    IAlbumRepository albumRepository,
    IStoredFileRepository storedFileRepository,
    IAlbumItemRepository albumItemRepository,
    TimeProvider timeProvider)
    : ICommandHandler<AddFileToAlbumCommand, Result<AddFileToAlbumResult>>
{
    public async ValueTask<Result<AddFileToAlbumResult>> Handle(
        AddFileToAlbumCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var album = await albumRepository.GetByIdAsync(command.AlbumId, cancellationToken);
        if (album is null)
        {
            return DomainError.NotFound(DomainErrorCodes.AlbumNotFound, "Album not found");
        }

        if (album.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Album belongs to a different family");
        }

        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken);
        if (file is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        if (file.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "File belongs to a different family");
        }

        var exists = await albumItemRepository.ExistsAsync(command.AlbumId, command.FileId, cancellationToken);
        if (exists)
        {
            return new AddFileToAlbumResult(true);
        }

        var item = AlbumItem.Create(command.AlbumId, command.FileId, command.UserId, utcNow);
        await albumItemRepository.AddAsync(item, cancellationToken);

        if (album.CoverFileId is null)
        {
            album.SetCoverImage(command.FileId, utcNow);
        }

        return new AddFileToAlbumResult(true);
    }
}
