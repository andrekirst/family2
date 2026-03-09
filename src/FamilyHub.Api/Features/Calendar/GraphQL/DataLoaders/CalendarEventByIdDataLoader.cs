using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Calendar.GraphQL.DataLoaders;

/// <summary>
/// Batch DataLoader for loading CalendarEvent entities by their IDs.
/// Eliminates N+1 queries when multiple CalendarEvent references are resolved in a single GraphQL request.
/// </summary>
public sealed class CalendarEventByIdDataLoader : BatchDataLoader<CalendarEventId, CalendarEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CalendarEventByIdDataLoader(
        IServiceScopeFactory scopeFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task<IReadOnlyDictionary<CalendarEventId, CalendarEvent>> LoadBatchAsync(
        IReadOnlyList<CalendarEventId> keys, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var events = await context.CalendarEvents
            .Include(e => e.Attendees)
            .Where(e => keys.Contains(e.Id))
            .ToListAsync(cancellationToken);

        return events.ToDictionary(e => e.Id);
    }
}
