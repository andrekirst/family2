using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Calendar.GraphQL.DataLoaders;

/// <summary>
/// Group DataLoader for loading CalendarEvent entities grouped by FamilyId.
/// Eliminates N+1 queries when resolving calendar events for multiple families in a single GraphQL request.
/// Only returns non-cancelled events ordered by start time.
/// </summary>
public sealed class CalendarEventsByFamilyIdDataLoader : GroupedDataLoader<FamilyId, CalendarEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CalendarEventsByFamilyIdDataLoader(
        IServiceScopeFactory scopeFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task<ILookup<FamilyId, CalendarEvent>> LoadGroupedBatchAsync(
        IReadOnlyList<FamilyId> keys, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var events = await context.CalendarEvents
            .Include(e => e.Attendees)
            .Where(e => keys.Contains(e.FamilyId) && !e.IsCancelled)
            .OrderBy(e => e.StartTime)
            .ToListAsync(cancellationToken);

        return events.ToLookup(e => e.FamilyId);
    }
}
