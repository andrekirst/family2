using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.BaseData.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.BaseData.Application.Queries.GetFederalStateByIso3166;

[ExtendObjectType(typeof(BaseDataQuery))]
public class QueryType
{
    [Authorize]
    public async Task<FederalStateDto?> GetFederalStateByIso3166(
        [Service] IQueryBus queryBus,
        string code,
        CancellationToken cancellationToken)
    {
        var query = new GetFederalStateByIso3166Query(code);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
