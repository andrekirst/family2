using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateAlbum;

public sealed class CreateAlbumCommandHandler(
    IAlbumRepository albumRepository)
    : ICommandHandler<CreateAlbumCommand, CreateAlbumResult>
{
    public async ValueTask<CreateAlbumResult> Handle(
        CreateAlbumCommand command,
        CancellationToken cancellationToken)
    {
        var album = Album.Create(command.Name, command.Description, command.FamilyId, command.CreatedBy);
        await albumRepository.AddAsync(album, cancellationToken);

        return new CreateAlbumResult(album.Id);
    }
}
