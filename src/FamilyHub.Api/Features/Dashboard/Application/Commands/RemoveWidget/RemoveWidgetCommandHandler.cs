using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.RemoveWidget;

public sealed class RemoveWidgetCommandHandler(
    IDashboardLayoutRepository dashboardRepository,
    TimeProvider timeProvider)
    : ICommandHandler<RemoveWidgetCommand, bool>
{
    public async ValueTask<bool> Handle(
        RemoveWidgetCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var dashboard = (await dashboardRepository.GetByWidgetIdAsync(command.WidgetId, cancellationToken))!;

        dashboard.RemoveWidget(command.WidgetId, utcNow);
        await dashboardRepository.UpdateAsync(dashboard, cancellationToken);

        return true;
    }
}
