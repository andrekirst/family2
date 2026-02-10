using FamilyHub.Api.Features.Calendar.Application.Commands;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;

namespace FamilyHub.Api.Features.Calendar.Application.Handlers;

public static class CreateCalendarEventCommandHandler
{
    public static async Task<CreateCalendarEventResult> Handle(
        CreateCalendarEventCommand command,
        ICalendarEventRepository repository,
        CancellationToken ct)
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

        await repository.AddAsync(calendarEvent, ct);
        await repository.SaveChangesAsync(ct);

        return new CreateCalendarEventResult(calendarEvent.Id);
    }
}
