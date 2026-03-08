using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateSecureNote;

public sealed record UpdateSecureNoteCommand(
    SecureNoteId NoteId,
    NoteCategory Category,
    string EncryptedTitle,
    string EncryptedContent,
    string Iv
) : ICommand<UpdateSecureNoteResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
