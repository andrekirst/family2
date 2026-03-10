using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFolder;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<FolderDto?> GetFolder(
        Guid folderId,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetFolderQuery(FolderId.From(folderId));
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
