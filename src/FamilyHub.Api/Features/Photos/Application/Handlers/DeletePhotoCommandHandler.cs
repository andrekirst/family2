using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Photos.Application.Commands;
using FamilyHub.Api.Features.Photos.Domain.Repositories;

namespace FamilyHub.Api.Features.Photos.Application.Handlers;

public sealed class DeletePhotoCommandHandler(
    IPhotoRepository repository)
    : ICommandHandler<DeletePhotoCommand, DeletePhotoResult>
{
    public async ValueTask<DeletePhotoResult> Handle(
        DeletePhotoCommand command,
        CancellationToken cancellationToken)
    {
        var photo = await repository.GetByIdAsync(command.PhotoId, cancellationToken)
            ?? throw new DomainException("Photo not found", DomainErrorCodes.PhotoNotFound);

        photo.SoftDelete(command.DeletedBy);
        await repository.SaveChangesAsync(cancellationToken);

        return new DeletePhotoResult(true);
    }
}
