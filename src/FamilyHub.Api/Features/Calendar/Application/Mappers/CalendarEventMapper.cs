using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Models;

namespace FamilyHub.Api.Features.Calendar.Application.Mappers;

public static class CalendarEventMapper
{
    public static CalendarEventDto ToDto(CalendarEvent calendarEvent)
    {
        return new CalendarEventDto
        {
            Id = calendarEvent.Id.Value,
            FamilyId = calendarEvent.FamilyId.Value,
            CreatedBy = calendarEvent.CreatedBy.Value,
            Title = calendarEvent.Title.Value,
            Description = calendarEvent.Description,
            Location = calendarEvent.Location,
            StartTime = calendarEvent.StartTime,
            EndTime = calendarEvent.EndTime,
            IsAllDay = calendarEvent.IsAllDay,
            Type = calendarEvent.Type.ToString(),
            IsCancelled = calendarEvent.IsCancelled,
            CreatedAt = calendarEvent.CreatedAt,
            UpdatedAt = calendarEvent.UpdatedAt,
            Attendees = calendarEvent.Attendees.Select(a => new CalendarEventAttendeeDto
            {
                UserId = a.UserId.Value
            }).ToList()
        };
    }
}
