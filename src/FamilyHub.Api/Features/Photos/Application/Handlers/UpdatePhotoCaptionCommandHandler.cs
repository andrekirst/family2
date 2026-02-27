using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Photos.Application.Commands;
using FamilyHub.Api.Features.Photos.Domain.Repositories;

namespace FamilyHub.Api.Features.Photos.Application.Handlers;

public sealed class UpdatePhotoCaptionCommandHandler(
    IPhotoRepository repository)
    : ICommandHandler<UpdatePhotoCaptionCommand, UpdatePhotoCaptionResult>
{
    public async ValueTask<UpdatePhotoCaptionResult> Handle(
        UpdatePhotoCaptionCommand command,
        CancellationToken cancellationToken)
    {
        var photo = await repository.GetByIdAsync(command.PhotoId, cancellationToken)
            ?? throw new DomainException("Photo not found", DomainErrorCodes.PhotoNotFound);

        photo.UpdateCaption(command.Caption);
        await repository.SaveChangesAsync(cancellationToken);

        return new UpdatePhotoCaptionResult(photo.Id);
    }
}
