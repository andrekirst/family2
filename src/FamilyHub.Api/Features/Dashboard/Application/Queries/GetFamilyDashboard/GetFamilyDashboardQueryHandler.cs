using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Application.Mappers;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetFamilyDashboard;

public sealed class GetFamilyDashboardQueryHandler(
    IDashboardLayoutRepository dashboardRepository)
    : IQueryHandler<GetFamilyDashboardQuery, DashboardLayoutDto?>
{
    public async ValueTask<DashboardLayoutDto?> Handle(
        GetFamilyDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var dashboard = await dashboardRepository.GetSharedDashboardAsync(query.FamilyId, cancellationToken);
        return dashboard is not null ? DashboardMapper.ToDto(dashboard) : null;
    }
}
