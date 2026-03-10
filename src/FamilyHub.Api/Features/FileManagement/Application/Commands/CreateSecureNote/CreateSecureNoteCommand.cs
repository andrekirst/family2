using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateSecureNote;

public sealed record CreateSecureNoteCommand(
    NoteCategory Category,
    string EncryptedTitle,
    string EncryptedContent,
    string Iv,
    string Salt,
    string Sentinel
) : ICommand<Result<CreateSecureNoteResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
