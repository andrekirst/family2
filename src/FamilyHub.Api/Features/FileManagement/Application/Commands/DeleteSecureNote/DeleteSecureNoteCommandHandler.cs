using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSecureNote;

public sealed class DeleteSecureNoteCommandHandler(
    ISecureNoteRepository noteRepository)
    : ICommandHandler<DeleteSecureNoteCommand, DeleteSecureNoteResult>
{
    public async ValueTask<DeleteSecureNoteResult> Handle(
        DeleteSecureNoteCommand command,
        CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken)
            ?? throw new DomainException("Secure note not found", DomainErrorCodes.SecureNoteNotFound);

        if (note.UserId != command.UserId)
            throw new DomainException("Secure note not found", DomainErrorCodes.SecureNoteNotFound);

        note.MarkDeleted();
        await noteRepository.RemoveAsync(note, cancellationToken);

        return new DeleteSecureNoteResult(true);
    }
}
