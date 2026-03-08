using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetMyFamily;

[ExtendObjectType(typeof(MeQuery))]
public class QueryType
{
    /// <summary>
    /// Get the current user's family.
    /// </summary>
    public async Task<FamilyDto?> GetFamily(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetMyFamilyQuery();

        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
