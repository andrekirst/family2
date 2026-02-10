using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Commands;

public sealed record UpdateCalendarEventResult(
    CalendarEventId CalendarEventId
);
