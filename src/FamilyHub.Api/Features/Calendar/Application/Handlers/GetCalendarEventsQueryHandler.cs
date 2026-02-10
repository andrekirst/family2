using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Calendar.Application.Mappers;
using FamilyHub.Api.Features.Calendar.Application.Queries;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.Models;

namespace FamilyHub.Api.Features.Calendar.Application.Handlers;

public sealed class GetCalendarEventsQueryHandler(
    ICalendarEventRepository repository)
    : IQueryHandler<GetCalendarEventsQuery, List<CalendarEventDto>>
{
    public async ValueTask<List<CalendarEventDto>> Handle(
        GetCalendarEventsQuery query,
        CancellationToken cancellationToken)
    {
        var events = await repository.GetByFamilyAndDateRangeAsync(
            query.FamilyId, query.StartDate, query.EndDate, cancellationToken);

        return events.Select(CalendarEventMapper.ToDto).ToList();
    }
}
