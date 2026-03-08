using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetTags;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<TagDto>> GetTags(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetTagsQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
