using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Domain.Repositories;

public interface IDashboardLayoutRepository : IWriteRepository<DashboardLayout, DashboardId>
{
    Task<bool> ExistsByWidgetIdAsync(DashboardWidgetId widgetId, CancellationToken cancellationToken = default);
    Task<DashboardLayout?> GetPersonalDashboardAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<DashboardLayout?> GetSharedDashboardAsync(FamilyId familyId, CancellationToken cancellationToken = default);
    Task UpdateAsync(DashboardLayout layout, CancellationToken cancellationToken = default);
    Task DeleteAsync(DashboardLayout layout, CancellationToken cancellationToken = default);
    Task<DashboardLayout?> GetByWidgetIdAsync(DashboardWidgetId widgetId, CancellationToken cancellationToken = default);
}
