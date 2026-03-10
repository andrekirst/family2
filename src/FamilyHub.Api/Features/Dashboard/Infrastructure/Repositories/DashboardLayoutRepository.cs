using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Dashboard.Infrastructure.Repositories;

public sealed class DashboardLayoutRepository(AppDbContext context) : IDashboardLayoutRepository
{
    public async Task<DashboardLayout?> GetByIdAsync(DashboardId id, CancellationToken cancellationToken = default)
    {
        return await context.DashboardLayouts
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(DashboardId id, CancellationToken cancellationToken = default)
    {
        return await context.DashboardLayouts.AnyAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByWidgetIdAsync(DashboardWidgetId widgetId, CancellationToken cancellationToken = default)
    {
        return await context.DashboardLayouts
            .AnyAsync(d => d.Widgets.Any(w => w.Id == widgetId), cancellationToken);
    }

    public async Task<DashboardLayout?> GetPersonalDashboardAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await context.DashboardLayouts
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsShared, cancellationToken);
    }

    public async Task<DashboardLayout?> GetSharedDashboardAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.DashboardLayouts
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.FamilyId == familyId && d.IsShared, cancellationToken);
    }

    public async Task AddAsync(DashboardLayout layout, CancellationToken cancellationToken = default)
    {
        await context.DashboardLayouts.AddAsync(layout, cancellationToken);
    }

    public Task UpdateAsync(DashboardLayout layout, CancellationToken cancellationToken = default)
    {
        // EF Core change tracker detects modifications automatically
        return Task.CompletedTask;
    }

    public async Task<DashboardLayout?> GetByWidgetIdAsync(DashboardWidgetId widgetId, CancellationToken cancellationToken = default)
    {
        return await context.DashboardLayouts
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Widgets.Any(w => w.Id == widgetId), cancellationToken);
    }

    public Task DeleteAsync(DashboardLayout layout, CancellationToken cancellationToken = default)
    {
        context.DashboardLayouts.Remove(layout);
        return Task.CompletedTask;
    }
}
