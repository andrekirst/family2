using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.AddWidget;

[ExtendObjectType(typeof(DashboardMutation))]
public class MutationType
{
    [Authorize]
    public async Task<DashboardWidgetDto> AddWidget(
        AddWidgetRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new AddWidgetCommand(
            DashboardId.From(input.DashboardId),
            WidgetTypeId.From(input.WidgetType),
            input.X, input.Y,
            input.Width, input.Height,
            input.ConfigJson);

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
