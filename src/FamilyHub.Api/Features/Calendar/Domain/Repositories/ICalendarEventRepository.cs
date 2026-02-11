using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Domain.Repositories;

public interface ICalendarEventRepository
{
    Task<CalendarEvent?> GetByIdAsync(CalendarEventId id, CancellationToken ct = default);
    Task<CalendarEvent?> GetByIdWithAttendeesAsync(CalendarEventId id, CancellationToken ct = default);
    Task<List<CalendarEvent>> GetByFamilyAndDateRangeAsync(FamilyId familyId, DateTime start, DateTime end, CancellationToken ct = default);
    Task AddAsync(CalendarEvent calendarEvent, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
