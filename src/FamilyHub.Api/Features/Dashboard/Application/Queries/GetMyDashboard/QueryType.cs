using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Dashboard.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetMyDashboard;

[ExtendObjectType(typeof(DashboardQuery))]
public class QueryType
{
    [Authorize]
    public async Task<DashboardLayoutDto?> MyDashboard(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetMyDashboardQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
