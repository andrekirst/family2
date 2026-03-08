using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.RemoveWidget;

public sealed class RemoveWidgetCommandHandler(
    IDashboardLayoutRepository dashboardRepository)
    : ICommandHandler<RemoveWidgetCommand, bool>
{
    public async ValueTask<bool> Handle(
        RemoveWidgetCommand command,
        CancellationToken cancellationToken)
    {
        var dashboard = (await dashboardRepository.GetByWidgetIdAsync(command.WidgetId, cancellationToken))!;

        dashboard.RemoveWidget(command.WidgetId);
        await dashboardRepository.UpdateAsync(dashboard, cancellationToken);
        await dashboardRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
