using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateAlbum;

public sealed class CreateAlbumCommandHandler(
    IAlbumRepository albumRepository,
    TimeProvider timeProvider)
    : ICommandHandler<CreateAlbumCommand, Result<CreateAlbumResult>>
{
    public async ValueTask<Result<CreateAlbumResult>> Handle(
        CreateAlbumCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var album = Album.Create(command.Name, command.Description, command.FamilyId, command.UserId, utcNow);
        await albumRepository.AddAsync(album, cancellationToken);

        return new CreateAlbumResult(album.Id);
    }
}
