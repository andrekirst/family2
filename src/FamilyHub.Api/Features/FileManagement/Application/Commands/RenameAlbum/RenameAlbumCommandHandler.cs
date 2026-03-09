using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameAlbum;

public sealed class RenameAlbumCommandHandler(
    IAlbumRepository albumRepository,
    TimeProvider timeProvider)
    : ICommandHandler<RenameAlbumCommand, Result<RenameAlbumResult>>
{
    public async ValueTask<Result<RenameAlbumResult>> Handle(
        RenameAlbumCommand command,
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

        album.Rename(command.NewName, utcNow);

        return new RenameAlbumResult(album.Id);
    }
}
