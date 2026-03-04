using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeCalendarEventRepository(List<CalendarEvent>? existingEvents = null) : ICalendarEventRepository
{
    private readonly List<CalendarEvent> _events = existingEvents ?? [];
    public List<CalendarEvent> AddedEvents { get; } = [];

    public Task<CalendarEvent?> GetByIdAsync(CalendarEventId id, CancellationToken ct = default) =>
        Task.FromResult(_events.Concat(AddedEvents).FirstOrDefault(e => e.Id == id));

    public Task<CalendarEvent?> GetByIdWithAttendeesAsync(CalendarEventId id, CancellationToken ct = default) =>
        GetByIdAsync(id, ct);

    public Task<List<CalendarEvent>> GetByFamilyAndDateRangeAsync(
        FamilyId familyId, DateTime start, DateTime end, CancellationToken ct = default) =>
        Task.FromResult(_events.Concat(AddedEvents)
            .Where(e => e.FamilyId == familyId && e.StartTime >= start && e.StartTime <= end)
            .OrderBy(e => e.StartTime)
            .ToList());

    public Task AddAsync(CalendarEvent calendarEvent, CancellationToken ct = default)
    {
        AddedEvents.Add(calendarEvent);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Task.FromResult(1);
}
