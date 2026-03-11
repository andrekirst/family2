using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.BaseData.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.BaseData.Application.Queries.GetFederalStates;

[ExtendObjectType(typeof(BaseDataQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<FederalStateDto>> GetFederalStates(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetFederalStatesQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
