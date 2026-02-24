using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateSecureNote;

public sealed record CreateSecureNoteCommand(
    FamilyId FamilyId,
    UserId UserId,
    NoteCategory Category,
    string EncryptedTitle,
    string EncryptedContent,
    string Iv,
    string Salt,
    string Sentinel
) : ICommand<CreateSecureNoteResult>;
