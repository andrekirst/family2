using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateSecureNote;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<object> CreateSecureNote(
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

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match<object>(
            success => success,
            error => MutationError.FromDomainError(error));
    }
}
