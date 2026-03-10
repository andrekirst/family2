using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Dashboard.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetFamilyDashboard;

[ExtendObjectType(typeof(DashboardQuery))]
public class QueryType
{
    [Authorize]
    public async Task<DashboardLayoutDto?> FamilyDashboard(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetFamilyDashboardQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
