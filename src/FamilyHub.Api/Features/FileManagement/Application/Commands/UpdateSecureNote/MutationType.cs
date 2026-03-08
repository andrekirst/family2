using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateSecureNote;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<UpdateSecureNoteResult> UpdateSecureNote(
        Guid noteId,
        string category,
        string encryptedTitle,
        string encryptedContent,
        string iv,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var noteCategory = Enum.Parse<NoteCategory>(category, ignoreCase: true);

        var command = new UpdateSecureNoteCommand(
            SecureNoteId.From(noteId),
            noteCategory,
            encryptedTitle,
            encryptedContent,
            iv);

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
