using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Commands;

public sealed record UpdateCalendarEventCommand(
    CalendarEventId CalendarEventId,
    EventTitle Title,
    string? Description,
    string? Location,
    DateTime StartTime,
    DateTime EndTime,
    bool IsAllDay,
    List<UserId> AttendeeIds
) : ICommand<UpdateCalendarEventResult>;
