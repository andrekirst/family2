using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Application.Mappers;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.UpdateWidgetConfig;

public sealed class UpdateWidgetConfigCommandHandler(
    IDashboardLayoutRepository dashboardRepository,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateWidgetConfigCommand, DashboardWidgetDto>
{
    public async ValueTask<DashboardWidgetDto> Handle(
        UpdateWidgetConfigCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var dashboard = (await dashboardRepository.GetByWidgetIdAsync(command.WidgetId, cancellationToken))!;

        var widget = dashboard.Widgets.First(w => w.Id == command.WidgetId);

        widget.UpdateConfig(command.ConfigJson, utcNow);
        await dashboardRepository.UpdateAsync(dashboard, cancellationToken);

        return DashboardMapper.ToDto(widget);
    }
}
