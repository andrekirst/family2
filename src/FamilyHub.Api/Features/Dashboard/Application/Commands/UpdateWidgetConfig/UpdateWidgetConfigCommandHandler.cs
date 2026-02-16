using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Dashboard.Application.Mappers;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.UpdateWidgetConfig;

public sealed class UpdateWidgetConfigCommandHandler(
    IDashboardLayoutRepository dashboardRepository)
    : ICommandHandler<UpdateWidgetConfigCommand, DashboardWidgetDto>
{
    public async ValueTask<DashboardWidgetDto> Handle(
        UpdateWidgetConfigCommand command,
        CancellationToken cancellationToken)
    {
        var dashboard = await dashboardRepository.GetByWidgetIdAsync(command.WidgetId, cancellationToken)
            ?? throw new DomainException($"Widget {command.WidgetId} not found");

        var widget = dashboard.Widgets.FirstOrDefault(w => w.Id == command.WidgetId)
            ?? throw new DomainException($"Widget {command.WidgetId} not found");

        widget.UpdateConfig(command.ConfigJson);
        await dashboardRepository.UpdateAsync(dashboard, cancellationToken);
        await dashboardRepository.SaveChangesAsync(cancellationToken);

        return DashboardMapper.ToDto(widget);
    }
}
