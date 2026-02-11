using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Models;

namespace FamilyHub.Api.Features.Calendar.Application.Queries;

public sealed record GetCalendarEventQuery(
    CalendarEventId CalendarEventId
) : IQuery<CalendarEventDto?>;
