using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Application.Commands;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;

namespace FamilyHub.Api.Features.Calendar.Application.Handlers;

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
            command.CreatedBy,
            command.Title,
            command.Description,
            command.Location,
            command.StartTime,
            command.EndTime,
            command.IsAllDay,
            command.Type);

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
        await repository.SaveChangesAsync(cancellationToken);

        return new CreateCalendarEventResult(calendarEvent.Id);
    }
}
