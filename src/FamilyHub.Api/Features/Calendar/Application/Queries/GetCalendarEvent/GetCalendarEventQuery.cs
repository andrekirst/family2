using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Queries.GetCalendarEvent;

public sealed record GetCalendarEventQuery(
    CalendarEventId CalendarEventId
) : IReadOnlyQuery<CalendarEventDto?>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
