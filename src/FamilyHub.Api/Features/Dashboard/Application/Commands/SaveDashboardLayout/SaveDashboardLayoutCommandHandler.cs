using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.SaveDashboardLayout;

public sealed class SaveDashboardLayoutCommandHandler(
    IDashboardLayoutRepository dashboardRepository)
    : ICommandHandler<SaveDashboardLayoutCommand, SaveDashboardLayoutResult>
{
    public async ValueTask<SaveDashboardLayoutResult> Handle(
        SaveDashboardLayoutCommand command,
        CancellationToken cancellationToken)
    {
        // Get or create dashboard
        DashboardLayout? dashboard;
        var isNew = false;

        if (command.IsShared)
        {
            dashboard = await dashboardRepository.GetSharedDashboardAsync(command.FamilyId, cancellationToken);
            if (dashboard is null)
            {
                dashboard = DashboardLayout.CreateShared(command.Name, command.FamilyId, command.UserId);
                isNew = true;
            }
        }
        else
        {
            dashboard = await dashboardRepository.GetPersonalDashboardAsync(command.UserId, cancellationToken);
            if (dashboard is null)
            {
                dashboard = DashboardLayout.CreatePersonal(command.Name, command.UserId);
                isNew = true;
            }
        }

        // Build new widget list
        var newWidgets = command.Widgets.Select(w =>
            DashboardWidget.Create(dashboard.Id, w.WidgetType, w.X, w.Y, w.Width, w.Height, w.SortOrder, w.ConfigJson))
            .ToList();

        dashboard.ReplaceAllWidgets(newWidgets);
        dashboard.UpdateName(command.Name);

        // Persist
        if (isNew)
        {
            await dashboardRepository.AddAsync(dashboard, cancellationToken);
        }
        else
        {
            await dashboardRepository.UpdateAsync(dashboard, cancellationToken);
        }

        return new SaveDashboardLayoutResult(dashboard.Id, dashboard);
    }
}
