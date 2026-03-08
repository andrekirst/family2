using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFolders;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<FolderDto>> GetFolders(
        Guid? parentFolderId,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetFoldersQuery(
            parentFolderId.HasValue && parentFolderId.Value != Guid.Empty
                ? FolderId.From(parentFolderId.Value)
                : null);

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
