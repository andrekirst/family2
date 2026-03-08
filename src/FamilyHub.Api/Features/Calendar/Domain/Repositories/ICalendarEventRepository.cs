using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Domain.Repositories;

public interface ICalendarEventRepository : IWriteRepository<CalendarEvent, CalendarEventId>
{
    Task<CalendarEvent?> GetByIdWithAttendeesAsync(CalendarEventId id, CancellationToken ct = default);
    Task<bool> IsCancelledAsync(CalendarEventId id, CancellationToken ct = default);
    Task<List<CalendarEvent>> GetByFamilyAndDateRangeAsync(FamilyId familyId, DateTime start, DateTime end, CancellationToken ct = default);
}
