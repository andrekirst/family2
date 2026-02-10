using FamilyHub.Api.Features.Calendar.Application.Mappers;
using FamilyHub.Api.Features.Calendar.Application.Queries;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.Models;

namespace FamilyHub.Api.Features.Calendar.Application.Handlers;

public static class GetCalendarEventQueryHandler
{
    public static async Task<CalendarEventDto?> Handle(
        GetCalendarEventQuery query,
        ICalendarEventRepository repository,
        CancellationToken ct)
    {
        var calendarEvent = await repository.GetByIdWithAttendeesAsync(query.CalendarEventId, ct);

        return calendarEvent is not null ? CalendarEventMapper.ToDto(calendarEvent) : null;
    }
}
