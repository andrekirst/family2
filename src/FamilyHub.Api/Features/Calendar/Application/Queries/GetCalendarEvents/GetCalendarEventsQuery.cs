using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Calendar.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Queries.GetCalendarEvents;

public sealed record GetCalendarEventsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IReadOnlyQuery<List<CalendarEventDto>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
