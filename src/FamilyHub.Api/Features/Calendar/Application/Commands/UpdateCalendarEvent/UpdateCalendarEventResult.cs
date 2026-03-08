using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.UpdateCalendarEvent;

public sealed record UpdateCalendarEventResult(
    CalendarEventId CalendarEventId
);
