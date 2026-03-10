using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateSecureNote;

public sealed class UpdateSecureNoteCommandHandler(
    ISecureNoteRepository noteRepository,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateSecureNoteCommand, Result<UpdateSecureNoteResult>>
{
    public async ValueTask<Result<UpdateSecureNoteResult>> Handle(
        UpdateSecureNoteCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken);
        if (note is null)
        {
            return DomainError.NotFound(DomainErrorCodes.SecureNoteNotFound, "Secure note not found");
        }

        if (note.UserId != command.UserId)
        {
            return DomainError.NotFound(DomainErrorCodes.SecureNoteNotFound, "Secure note not found");
        }

        note.Update(
            command.Category,
            command.EncryptedTitle,
            command.EncryptedContent,
            command.Iv,
            utcNow);

        return new UpdateSecureNoteResult(true);
    }
}
