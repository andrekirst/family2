using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Calendar.Application.Queries;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Models;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Calendar.GraphQL;

[ExtendObjectType(typeof(FamilyQuery))]
public class CalendarQueries
{
    [Authorize]
    public async Task<List<CalendarEventDto>> GetCalendars(
        Guid familyId,
        DateTime startDate,
        DateTime endDate,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var query = new GetCalendarEventsQuery(
            FamilyId.From(familyId),
            startDate,
            endDate);

        return await queryBus.QueryAsync<List<CalendarEventDto>>(query, ct);
    }

    [Authorize]
    public async Task<CalendarEventDto?> GetCalendar(
        Guid id,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var query = new GetCalendarEventQuery(CalendarEventId.From(id));
        return await queryBus.QueryAsync<CalendarEventDto?>(query, ct);
    }
}
