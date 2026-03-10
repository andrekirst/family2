using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetSavedSearches;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    [HotChocolate.Types.UsePaging]
    public async Task<List<SavedSearchDto>> GetSavedSearches(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetSavedSearchesQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
