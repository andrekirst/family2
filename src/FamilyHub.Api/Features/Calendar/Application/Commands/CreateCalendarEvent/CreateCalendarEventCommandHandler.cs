using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.CreateCalendarEvent;

public sealed class CreateCalendarEventCommandHandler(
    ICalendarEventRepository repository)
    : ICommandHandler<CreateCalendarEventCommand, CreateCalendarEventResult>
{
    public async ValueTask<CreateCalendarEventResult> Handle(
        CreateCalendarEventCommand command,
        CancellationToken cancellationToken)
    {
        var calendarEvent = CalendarEvent.Create(
            command.FamilyId,
            command.UserId,
            command.Title,
            command.Description,
            command.Location,
            command.StartTime,
            command.EndTime,
            command.IsAllDay);

        // Add attendees
        foreach (var attendeeId in command.AttendeeIds)
        {
            calendarEvent.Attendees.Add(new CalendarEventAttendee
            {
                CalendarEventId = calendarEvent.Id,
                UserId = attendeeId
            });
        }

        await repository.AddAsync(calendarEvent, cancellationToken);

        return new CreateCalendarEventResult(calendarEvent.Id, calendarEvent);
    }
}
