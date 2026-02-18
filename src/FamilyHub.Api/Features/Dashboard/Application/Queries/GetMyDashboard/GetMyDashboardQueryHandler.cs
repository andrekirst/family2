using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Application.Mappers;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetMyDashboard;

public sealed class GetMyDashboardQueryHandler(
    IDashboardLayoutRepository dashboardRepository)
    : IQueryHandler<GetMyDashboardQuery, DashboardLayoutDto?>
{
    public async ValueTask<DashboardLayoutDto?> Handle(
        GetMyDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var dashboard = await dashboardRepository.GetPersonalDashboardAsync(query.UserId, cancellationToken);
        return dashboard is not null ? DashboardMapper.ToDto(dashboard) : null;
    }
}
