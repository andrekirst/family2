using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Dashboard.Application.Mappers;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.SaveDashboardLayout;

[ExtendObjectType(typeof(DashboardMutation))]
public class MutationType
{
    [Authorize]
    public async Task<DashboardLayoutDto> SaveLayout(
        SaveDashboardLayoutRequest input,
        [Service] ICommandBus commandBus,
        [Service] IDashboardLayoutRepository dashboardRepository,
        CancellationToken cancellationToken)
    {
        var widgets = input.Widgets.Select(w => new WidgetPositionData(
            WidgetTypeId.From(w.WidgetType),
            w.X, w.Y, w.Width, w.Height, w.SortOrder, w.ConfigJson))
            .ToList();

        var command = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From(input.Name.Trim()),
            input.IsShared,
            widgets);

        var result = await commandBus.SendAsync(command, cancellationToken);
        var dashboard = await dashboardRepository.GetByIdAsync(result.DashboardId, cancellationToken);

        return dashboard is null
            ? throw new InvalidOperationException("Dashboard save failed")
            : DashboardMapper.ToDto(dashboard);
    }
}
