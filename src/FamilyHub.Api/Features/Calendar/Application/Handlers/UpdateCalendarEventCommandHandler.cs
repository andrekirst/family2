using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Calendar.Application.Commands;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;

namespace FamilyHub.Api.Features.Calendar.Application.Handlers;

public static class UpdateCalendarEventCommandHandler
{
    public static async Task<UpdateCalendarEventResult> Handle(
        UpdateCalendarEventCommand command,
        ICalendarEventRepository repository,
        CancellationToken ct)
    {
        var calendarEvent = await repository.GetByIdWithAttendeesAsync(command.CalendarEventId, ct)
            ?? throw new DomainException("Calendar event not found");

        if (calendarEvent.IsCancelled)
        {
            throw new DomainException("Cannot update a cancelled event");
        }

        calendarEvent.Update(
            command.Title,
            command.Description,
            command.Location,
            command.StartTime,
            command.EndTime,
            command.IsAllDay,
            command.Type);

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

        await repository.SaveChangesAsync(ct);

        return new UpdateCalendarEventResult(calendarEvent.Id);
    }
}
