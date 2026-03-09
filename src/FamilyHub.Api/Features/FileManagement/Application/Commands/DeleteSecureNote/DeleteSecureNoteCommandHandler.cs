using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSecureNote;

public sealed class DeleteSecureNoteCommandHandler(
    ISecureNoteRepository noteRepository)
    : ICommandHandler<DeleteSecureNoteCommand, Result<DeleteSecureNoteResult>>
{
    public async ValueTask<Result<DeleteSecureNoteResult>> Handle(
        DeleteSecureNoteCommand command,
        CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken);
        if (note is null)
        {
            return DomainError.NotFound(DomainErrorCodes.SecureNoteNotFound, "Secure note not found");
        }

        if (note.UserId != command.UserId)
        {
            return DomainError.NotFound(DomainErrorCodes.SecureNoteNotFound, "Secure note not found");
        }

        note.MarkDeleted();
        await noteRepository.RemoveAsync(note, cancellationToken);

        return new DeleteSecureNoteResult(true);
    }
}
