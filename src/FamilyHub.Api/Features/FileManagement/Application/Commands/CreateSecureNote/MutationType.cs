using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateSecureNote;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<CreateSecureNoteResult> CreateSecureNote(
        string category,
        string encryptedTitle,
        string encryptedContent,
        string iv,
        string salt,
        string sentinel,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var noteCategory = Enum.Parse<NoteCategory>(category, ignoreCase: true);

        var command = new CreateSecureNoteCommand(
            noteCategory,
            encryptedTitle,
            encryptedContent,
            iv,
            salt,
            sentinel);

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
