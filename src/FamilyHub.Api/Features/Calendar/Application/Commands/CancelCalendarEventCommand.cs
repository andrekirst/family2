using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Commands;

public sealed record CancelCalendarEventCommand(
    CalendarEventId CalendarEventId
) : ICommand<CancelCalendarEventResult>;
