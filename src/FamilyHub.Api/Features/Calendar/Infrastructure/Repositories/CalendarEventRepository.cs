using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Calendar.Infrastructure.Repositories;

public sealed class CalendarEventRepository(AppDbContext context) : ICalendarEventRepository
{
    public async Task<CalendarEvent?> GetByIdAsync(CalendarEventId id, CancellationToken cancellationToken = default)
    {
        return await context.CalendarEvents.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<CalendarEvent?> GetByIdWithAttendeesAsync(CalendarEventId id, CancellationToken cancellationToken = default)
    {
        return await context.CalendarEvents
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(CalendarEventId id, CancellationToken cancellationToken = default)
    {
        return await context.CalendarEvents.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> IsCancelledAsync(CalendarEventId id, CancellationToken cancellationToken = default)
    {
        var calendarEvent = await context.CalendarEvents
            .Where(e => e.Id == id)
            .Select(e => new { e.IsCancelled })
            .FirstOrDefaultAsync(cancellationToken);

        return calendarEvent?.IsCancelled
            ?? throw new EntityNotFoundException<CalendarEvent>(id);
    }

    public async Task<List<CalendarEvent>> GetByFamilyAndDateRangeAsync(
        FamilyId familyId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await context.CalendarEvents
            .Include(e => e.Attendees)
            .AsSplitQuery()
            .Where(e => e.FamilyId == familyId
                && e.StartTime < end
                && e.EndTime > start
                && !e.IsCancelled)
            .OrderBy(e => e.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        await context.CalendarEvents.AddAsync(calendarEvent, cancellationToken);
    }
}
