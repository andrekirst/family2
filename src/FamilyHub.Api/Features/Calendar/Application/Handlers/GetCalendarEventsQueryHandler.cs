using FamilyHub.Api.Features.Calendar.Application.Mappers;
using FamilyHub.Api.Features.Calendar.Application.Queries;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.Models;

namespace FamilyHub.Api.Features.Calendar.Application.Handlers;

public static class GetCalendarEventsQueryHandler
{
    public static async Task<List<CalendarEventDto>> Handle(
        GetCalendarEventsQuery query,
        ICalendarEventRepository repository,
        CancellationToken ct)
    {
        var events = await repository.GetByFamilyAndDateRangeAsync(
            query.FamilyId, query.StartDate, query.EndDate, ct);

        return events.Select(CalendarEventMapper.ToDto).ToList();
    }
}
