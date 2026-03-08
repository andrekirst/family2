using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeCalendarEventRepository(List<CalendarEvent>? existingEvents = null) : ICalendarEventRepository
{
    private readonly List<CalendarEvent> _events = existingEvents ?? [];
    public List<CalendarEvent> AddedEvents { get; } = [];

    private IEnumerable<CalendarEvent> All => _events.Concat(AddedEvents);

    public Task<CalendarEvent?> GetByIdAsync(CalendarEventId id, CancellationToken ct = default) =>
        Task.FromResult(All.FirstOrDefault(e => e.Id == id));

    public Task<CalendarEvent?> GetByIdWithAttendeesAsync(CalendarEventId id, CancellationToken ct = default) =>
        GetByIdAsync(id, ct);

    public Task<bool> ExistsByIdAsync(CalendarEventId id, CancellationToken ct = default) =>
        Task.FromResult(All.Any(e => e.Id == id));

    public Task<bool> IsCancelledAsync(CalendarEventId id, CancellationToken ct = default)
    {
        var calendarEvent = All.FirstOrDefault(e => e.Id == id)
            ?? throw new EntityNotFoundException<CalendarEvent>(id);
        return Task.FromResult(calendarEvent.IsCancelled);
    }

    public Task<List<CalendarEvent>> GetByFamilyAndDateRangeAsync(
        FamilyId familyId, DateTime start, DateTime end, CancellationToken ct = default) =>
        Task.FromResult(All
            .Where(e => e.FamilyId == familyId && e.StartTime >= start && e.StartTime <= end)
            .OrderBy(e => e.StartTime)
            .ToList());

    public Task AddAsync(CalendarEvent calendarEvent, CancellationToken ct = default)
    {
        AddedEvents.Add(calendarEvent);
        return Task.CompletedTask;
    }
}
