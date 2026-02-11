using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Calendar.Application.Mappers;
using FamilyHub.Api.Features.Calendar.Application.Queries;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.Models;

namespace FamilyHub.Api.Features.Calendar.Application.Handlers;

public sealed class GetCalendarEventQueryHandler(
    ICalendarEventRepository repository)
    : IQueryHandler<GetCalendarEventQuery, CalendarEventDto?>
{
    public async ValueTask<CalendarEventDto?> Handle(
        GetCalendarEventQuery query,
        CancellationToken cancellationToken)
    {
        var calendarEvent = await repository.GetByIdWithAttendeesAsync(query.CalendarEventId, cancellationToken);

        return calendarEvent is not null ? CalendarEventMapper.ToDto(calendarEvent) : null;
    }
}
