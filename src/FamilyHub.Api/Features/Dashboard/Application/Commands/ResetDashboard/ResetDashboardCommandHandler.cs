using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.ResetDashboard;

public sealed class ResetDashboardCommandHandler(
    IDashboardLayoutRepository dashboardRepository)
    : ICommandHandler<ResetDashboardCommand, bool>
{
    public async ValueTask<bool> Handle(
        ResetDashboardCommand command,
        CancellationToken cancellationToken)
    {
        var dashboard = await dashboardRepository.GetByIdAsync(command.DashboardId, cancellationToken)
            ?? throw new DomainException($"Dashboard {command.DashboardId} not found");

        dashboard.ReplaceAllWidgets([]);
        await dashboardRepository.UpdateAsync(dashboard, cancellationToken);
        await dashboardRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
