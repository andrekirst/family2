using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFilesByTag;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    [HotChocolate.Types.UsePaging]
    public async Task<List<StoredFileDto>> GetFilesByTag(
        List<Guid> tagIds,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetFilesByTagQuery(
            tagIds.Select(TagId.From).ToList());
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
