using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.ResetDashboard;

public sealed class ResetDashboardCommandHandler(
    IDashboardLayoutRepository dashboardRepository,
    TimeProvider timeProvider)
    : ICommandHandler<ResetDashboardCommand, bool>
{
    public async ValueTask<bool> Handle(
        ResetDashboardCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var dashboard = (await dashboardRepository.GetByIdAsync(command.DashboardId, cancellationToken))!;

        dashboard.ReplaceAllWidgets([], utcNow);
        await dashboardRepository.UpdateAsync(dashboard, cancellationToken);

        return true;
    }
}
