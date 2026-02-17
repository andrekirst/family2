using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.ResetDashboard;

[ExtendObjectType(typeof(DashboardMutation))]
public class MutationType
{
    [Authorize]
    public async Task<bool> ResetDashboard(
        Guid dashboardId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new ResetDashboardCommand(DashboardId.From(dashboardId));
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
