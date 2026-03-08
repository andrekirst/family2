using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.UpdateCalendarEvent;

public sealed class UpdateCalendarEventCommandHandler(
    ICalendarEventRepository repository)
    : ICommandHandler<UpdateCalendarEventCommand, UpdateCalendarEventResult>
{
    public async ValueTask<UpdateCalendarEventResult> Handle(
        UpdateCalendarEventCommand command,
        CancellationToken cancellationToken)
    {
        var calendarEvent = (await repository.GetByIdWithAttendeesAsync(command.CalendarEventId, cancellationToken))!;

        calendarEvent.Update(
            command.Title,
            command.Description,
            command.Location,
            command.StartTime,
            command.EndTime,
            command.IsAllDay);

        // Replace attendees
        calendarEvent.Attendees.Clear();
        foreach (var attendeeId in command.AttendeeIds)
        {
            calendarEvent.Attendees.Add(new CalendarEventAttendee
            {
                CalendarEventId = calendarEvent.Id,
                UserId = attendeeId
            });
        }

        await repository.SaveChangesAsync(cancellationToken);

        return new UpdateCalendarEventResult(calendarEvent.Id, calendarEvent);
    }
}
