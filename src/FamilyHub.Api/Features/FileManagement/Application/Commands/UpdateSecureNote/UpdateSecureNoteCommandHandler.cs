using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateSecureNote;

public sealed class UpdateSecureNoteCommandHandler(
    ISecureNoteRepository noteRepository)
    : ICommandHandler<UpdateSecureNoteCommand, UpdateSecureNoteResult>
{
    public async ValueTask<UpdateSecureNoteResult> Handle(
        UpdateSecureNoteCommand command,
        CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken)
            ?? throw new DomainException("Secure note not found", DomainErrorCodes.SecureNoteNotFound);

        if (note.UserId != command.UserId)
            throw new DomainException("Secure note not found", DomainErrorCodes.SecureNoteNotFound);

        note.Update(
            command.Category,
            command.EncryptedTitle,
            command.EncryptedContent,
            command.Iv);

        return new UpdateSecureNoteResult(true);
    }
}
