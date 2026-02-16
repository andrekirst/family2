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

    public Task<DashboardLayout?> GetByIdAsync(DashboardId id, CancellationToken ct = default) =>
        Task.FromResult(_layouts.Concat(AddedLayouts).FirstOrDefault(d => d.Id == id));

    public Task<DashboardLayout?> GetPersonalDashboardAsync(UserId userId, CancellationToken ct = default) =>
        Task.FromResult(_layouts.Concat(AddedLayouts).FirstOrDefault(d => d.UserId == userId && !d.IsShared));

    public Task<DashboardLayout?> GetSharedDashboardAsync(FamilyId familyId, CancellationToken ct = default) =>
        Task.FromResult(_layouts.Concat(AddedLayouts).FirstOrDefault(d => d.FamilyId == familyId && d.IsShared));

    public Task<DashboardLayout?> GetByWidgetIdAsync(DashboardWidgetId widgetId, CancellationToken ct = default) =>
        Task.FromResult(_layouts.Concat(AddedLayouts).FirstOrDefault(d => d.Widgets.Any(w => w.Id == widgetId)));

    public Task AddAsync(DashboardLayout layout, CancellationToken ct = default)
    {
        AddedLayouts.Add(layout);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DashboardLayout layout, CancellationToken ct = default)
    {
        UpdatedLayouts.Add(layout);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(DashboardLayout layout, CancellationToken ct = default)
    {
        DeletedLayouts.Add(layout);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Task.FromResult(1);
}
