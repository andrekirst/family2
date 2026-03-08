using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Domain.Repositories;

public interface IDashboardLayoutRepository : IWriteRepository<DashboardLayout, DashboardId>
{
    Task<bool> ExistsByWidgetIdAsync(DashboardWidgetId widgetId, CancellationToken ct = default);
    Task<DashboardLayout?> GetPersonalDashboardAsync(UserId userId, CancellationToken ct = default);
    Task<DashboardLayout?> GetSharedDashboardAsync(FamilyId familyId, CancellationToken ct = default);
    Task UpdateAsync(DashboardLayout layout, CancellationToken ct = default);
    Task DeleteAsync(DashboardLayout layout, CancellationToken ct = default);
    Task<DashboardLayout?> GetByWidgetIdAsync(DashboardWidgetId widgetId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
