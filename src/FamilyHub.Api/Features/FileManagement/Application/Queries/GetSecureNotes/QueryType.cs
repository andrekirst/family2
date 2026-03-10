using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetSecureNotes;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    [HotChocolate.Types.UsePaging]
    public async Task<List<SecureNoteDto>> GetSecureNotes(
        string? category,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        NoteCategory? noteCategory = category is not null
            ? Enum.Parse<NoteCategory>(category, ignoreCase: true)
            : null;

        var query = new GetSecureNotesQuery(noteCategory);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
