using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.UpdateWidgetConfig;

[ExtendObjectType(typeof(DashboardMutation))]
public class MutationType
{
    [Authorize]
    public async Task<DashboardWidgetDto> UpdateWidgetConfig(
        UpdateWidgetConfigRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new UpdateWidgetConfigCommand(
            DashboardWidgetId.From(input.WidgetId),
            input.ConfigJson);

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
