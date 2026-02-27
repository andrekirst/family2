using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Photos.Application.Commands;
using FamilyHub.Api.Features.Photos.Domain.Entities;
using FamilyHub.Api.Features.Photos.Domain.Repositories;

namespace FamilyHub.Api.Features.Photos.Application.Handlers;

public sealed class UploadPhotoCommandHandler(
    IPhotoRepository repository)
    : ICommandHandler<UploadPhotoCommand, UploadPhotoResult>
{
    public async ValueTask<UploadPhotoResult> Handle(
        UploadPhotoCommand command,
        CancellationToken cancellationToken)
    {
        var photo = Photo.Create(
            command.FamilyId,
            command.UploadedBy,
            command.FileName,
            command.ContentType,
            command.FileSizeBytes,
            command.StoragePath,
            command.Caption);

        await repository.AddAsync(photo, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new UploadPhotoResult(photo.Id);
    }
}
