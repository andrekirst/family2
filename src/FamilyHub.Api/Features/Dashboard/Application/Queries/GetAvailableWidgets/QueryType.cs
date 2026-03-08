using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Dashboard.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetAvailableWidgets;

[ExtendObjectType(typeof(DashboardQuery))]
public class QueryType
{
    [Authorize]
    public async Task<IReadOnlyList<WidgetDescriptorDto>> AvailableWidgets(
        Guid familyId,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetAvailableWidgetsQuery(FamilyId.From(familyId));
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
