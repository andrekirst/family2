using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameAlbum;

public sealed class RenameAlbumCommandHandler(
    IAlbumRepository albumRepository)
    : ICommandHandler<RenameAlbumCommand, RenameAlbumResult>
{
    public async ValueTask<RenameAlbumResult> Handle(
        RenameAlbumCommand command,
        CancellationToken cancellationToken)
    {
        var album = await albumRepository.GetByIdAsync(command.AlbumId, cancellationToken)
            ?? throw new DomainException("Album not found", DomainErrorCodes.AlbumNotFound);

        if (album.FamilyId != command.FamilyId)
            throw new DomainException("Album belongs to a different family", DomainErrorCodes.Forbidden);

        album.Rename(command.NewName);

        return new RenameAlbumResult(album.Id);
    }
}
