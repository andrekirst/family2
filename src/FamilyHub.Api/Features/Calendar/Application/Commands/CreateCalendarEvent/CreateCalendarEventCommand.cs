using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.CreateCalendarEvent;

public sealed record CreateCalendarEventCommand(
    EventTitle Title,
    string? Description,
    string? Location,
    DateTime StartTime,
    DateTime EndTime,
    bool IsAllDay,
    List<UserId> AttendeeIds
) : ICommand<CreateCalendarEventResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
