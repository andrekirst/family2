using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Calendar.Application.Queries.GetCalendarEvent;
using FamilyHub.Api.Features.Calendar.Application.Queries.GetCalendarEvents;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Models;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Calendar.GraphQL;

[ExtendObjectType(typeof(FamilyQuery))]
public class CalendarQueries
{
    [Authorize]
    public async Task<List<CalendarEventDto>> GetCalendars(
        DateTime startDate,
        DateTime endDate,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var query = new GetCalendarEventsQuery(startDate, endDate);
        return await queryBus.QueryAsync(query, ct);
    }

    [Authorize]
    public async Task<CalendarEventDto?> GetCalendar(
        Guid id,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var query = new GetCalendarEventQuery(CalendarEventId.From(id));
        return await queryBus.QueryAsync(query, ct);
    }
}
