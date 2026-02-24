using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateSecureNote;

public sealed record UpdateSecureNoteCommand(
    SecureNoteId NoteId,
    UserId UserId,
    NoteCategory Category,
    string EncryptedTitle,
    string EncryptedContent,
    string Iv
) : ICommand<UpdateSecureNoteResult>;
