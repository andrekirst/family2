using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.CreateCalendarEvent;

public sealed record CreateCalendarEventResult(
    CalendarEventId CalendarEventId
);
