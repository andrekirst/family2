using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.RemoveWidget;

[ExtendObjectType(typeof(DashboardMutation))]
public class MutationType
{
    [Authorize]
    public async Task<bool> RemoveWidget(
        Guid widgetId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new RemoveWidgetCommand(DashboardWidgetId.From(widgetId));
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
