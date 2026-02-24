using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateSecureNote;

public sealed class CreateSecureNoteCommandHandler(
    ISecureNoteRepository noteRepository)
    : ICommandHandler<CreateSecureNoteCommand, CreateSecureNoteResult>
{
    public async ValueTask<CreateSecureNoteResult> Handle(
        CreateSecureNoteCommand command,
        CancellationToken cancellationToken)
    {
        var note = SecureNote.Create(
            command.FamilyId,
            command.UserId,
            command.Category,
            command.EncryptedTitle,
            command.EncryptedContent,
            command.Iv,
            command.Salt,
            command.Sentinel);

        await noteRepository.AddAsync(note, cancellationToken);

        return new CreateSecureNoteResult(note.Id.Value);
    }
}
