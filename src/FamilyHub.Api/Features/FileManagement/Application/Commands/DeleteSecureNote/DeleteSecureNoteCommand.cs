using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSecureNote;

public sealed record DeleteSecureNoteCommand(
    SecureNoteId NoteId
) : ICommand<DeleteSecureNoteResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
