using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Common.Widgets;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.SaveDashboardLayout;

public sealed class SaveDashboardLayoutCommandHandler(
    IDashboardLayoutRepository dashboardRepository,
    IWidgetRegistry widgetRegistry)
    : ICommandHandler<SaveDashboardLayoutCommand, SaveDashboardLayoutResult>
{
    public async ValueTask<SaveDashboardLayoutResult> Handle(
        SaveDashboardLayoutCommand command,
        CancellationToken cancellationToken)
    {
        // Validate all widget types
        foreach (var widget in command.Widgets)
        {
            if (!widgetRegistry.IsValidWidget(widget.WidgetType.Value))
                throw new DomainException($"Invalid widget type: {widget.WidgetType.Value}");
        }

        // Get or create dashboard
        DashboardLayout? dashboard;
        var isNew = false;

        if (command.IsShared && command.FamilyId.HasValue)
        {
            dashboard = await dashboardRepository.GetSharedDashboardAsync(command.FamilyId.Value, cancellationToken);
            if (dashboard is null)
            {
                dashboard = DashboardLayout.CreateShared(command.Name, command.FamilyId.Value, command.UserId!.Value);
                isNew = true;
            }
        }
        else if (command.UserId.HasValue)
        {
            dashboard = await dashboardRepository.GetPersonalDashboardAsync(command.UserId.Value, cancellationToken);
            if (dashboard is null)
            {
                dashboard = DashboardLayout.CreatePersonal(command.Name, command.UserId.Value);
                isNew = true;
            }
        }
        else
        {
            throw new DomainException("Either UserId or FamilyId must be provided");
        }

        // Build new widget list
        var newWidgets = command.Widgets.Select(w =>
            DashboardWidget.Create(dashboard.Id, w.WidgetType, w.X, w.Y, w.Width, w.Height, w.SortOrder, w.ConfigJson))
            .ToList();

        dashboard.ReplaceAllWidgets(newWidgets);
        dashboard.UpdateName(command.Name);

        // Persist
        if (isNew)
            await dashboardRepository.AddAsync(dashboard, cancellationToken);
        else
            await dashboardRepository.UpdateAsync(dashboard, cancellationToken);

        await dashboardRepository.SaveChangesAsync(cancellationToken);

        return new SaveDashboardLayoutResult(dashboard.Id);
    }
}
