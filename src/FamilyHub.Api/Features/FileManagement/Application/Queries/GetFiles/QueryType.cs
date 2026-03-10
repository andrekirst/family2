using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFiles;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    [HotChocolate.Types.UsePaging]
    public async Task<List<StoredFileDto>> GetFiles(
        Guid? folderId,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetFilesQuery(
            folderId.HasValue && folderId.Value != Guid.Empty
                ? FolderId.From(folderId.Value)
                : null);

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
