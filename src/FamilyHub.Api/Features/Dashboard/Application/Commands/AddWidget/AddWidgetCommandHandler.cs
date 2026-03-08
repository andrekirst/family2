using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Application.Mappers;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.AddWidget;

public sealed class AddWidgetCommandHandler(
    IDashboardLayoutRepository dashboardRepository)
    : ICommandHandler<AddWidgetCommand, DashboardWidgetDto>
{
    public async ValueTask<DashboardWidgetDto> Handle(
        AddWidgetCommand command,
        CancellationToken cancellationToken)
    {
        var dashboard = (await dashboardRepository.GetByIdAsync(command.DashboardId, cancellationToken))!;

        var sortOrder = dashboard.Widgets.Count;
        var widget = dashboard.AddWidget(
            command.WidgetType, command.X, command.Y,
            command.Width, command.Height, sortOrder, command.ConfigJson);

        await dashboardRepository.UpdateAsync(dashboard, cancellationToken);
        await dashboardRepository.SaveChangesAsync(cancellationToken);

        return DashboardMapper.ToDto(widget);
    }
}
