using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Calendar.Models;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Queries;

public sealed record GetCalendarEventsQuery(
    FamilyId FamilyId,
    DateTime StartDate,
    DateTime EndDate
) : IQuery<List<CalendarEventDto>>;
