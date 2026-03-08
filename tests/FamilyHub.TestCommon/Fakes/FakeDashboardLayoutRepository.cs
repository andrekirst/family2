using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeDashboardLayoutRepository : IDashboardLayoutRepository
{
    public List<DashboardLayout> AddedLayouts { get; } = [];
    public List<DashboardLayout> UpdatedLayouts { get; } = [];
    public List<DashboardLayout> DeletedLayouts { get; } = [];

    private readonly List<DashboardLayout> _layouts = [];

    public void Seed(DashboardLayout layout) => _layouts.Add(layout);

    private IEnumerable<DashboardLayout> All => All;

    public Task<DashboardLayout?> GetByIdAsync(DashboardId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.FirstOrDefault(d => d.Id == id));

    public Task<bool> ExistsByIdAsync(DashboardId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.Any(d => d.Id == id));

    public Task<bool> ExistsByWidgetIdAsync(DashboardWidgetId widgetId, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.Any(d => d.Widgets.Any(w => w.Id == widgetId)));

    public Task<DashboardLayout?> GetPersonalDashboardAsync(UserId userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.FirstOrDefault(d => d.UserId == userId && !d.IsShared));

    public Task<DashboardLayout?> GetSharedDashboardAsync(FamilyId familyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.FirstOrDefault(d => d.FamilyId == familyId && d.IsShared));

    public Task<DashboardLayout?> GetByWidgetIdAsync(DashboardWidgetId widgetId, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.FirstOrDefault(d => d.Widgets.Any(w => w.Id == widgetId)));

    public Task AddAsync(DashboardLayout layout, CancellationToken cancellationToken = default)
    {
        AddedLayouts.Add(layout);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DashboardLayout layout, CancellationToken cancellationToken = default)
    {
        UpdatedLayouts.Add(layout);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(DashboardLayout layout, CancellationToken cancellationToken = default)
    {
        DeletedLayouts.Add(layout);
        return Task.CompletedTask;
    }
}
