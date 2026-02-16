using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Dashboard.Infrastructure.Repositories;

public sealed class DashboardLayoutRepository(AppDbContext context) : IDashboardLayoutRepository
{
    public async Task<DashboardLayout?> GetByIdAsync(DashboardId id, CancellationToken ct = default)
    {
        return await context.DashboardLayouts
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<DashboardLayout?> GetPersonalDashboardAsync(UserId userId, CancellationToken ct = default)
    {
        return await context.DashboardLayouts
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsShared, ct);
    }

    public async Task<DashboardLayout?> GetSharedDashboardAsync(FamilyId familyId, CancellationToken ct = default)
    {
        return await context.DashboardLayouts
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.FamilyId == familyId && d.IsShared, ct);
    }

    public async Task AddAsync(DashboardLayout layout, CancellationToken ct = default)
    {
        await context.DashboardLayouts.AddAsync(layout, ct);
    }

    public Task UpdateAsync(DashboardLayout layout, CancellationToken ct = default)
    {
        context.DashboardLayouts.Update(layout);
        return Task.CompletedTask;
    }

    public async Task<DashboardLayout?> GetByWidgetIdAsync(DashboardWidgetId widgetId, CancellationToken ct = default)
    {
        return await context.DashboardLayouts
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Widgets.Any(w => w.Id == widgetId), ct);
    }

    public Task DeleteAsync(DashboardLayout layout, CancellationToken ct = default)
    {
        context.DashboardLayouts.Remove(layout);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}
