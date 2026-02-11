using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Calendar.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Queries;

public sealed record GetCalendarEventsQuery(
    FamilyId FamilyId,
    DateTime StartDate,
    DateTime EndDate
) : IQuery<List<CalendarEventDto>>;
