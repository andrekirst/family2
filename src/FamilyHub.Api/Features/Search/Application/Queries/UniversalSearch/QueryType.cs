using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Search.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Search.Application.Queries.UniversalSearch;

[ExtendObjectType(typeof(SearchQuery))]
public class QueryType
{
    [Authorize]
    public async Task<UniversalSearchResult> Universal(
        UniversalSearchRequest input,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new UniversalSearchQuery(
            Query: input.Query,
            Modules: input.Modules,
            Limit: input.Limit ?? 10,
            Locale: input.Locale);

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
