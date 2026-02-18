using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Domain.Events;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Domain.Entities;

public sealed class CalendarEvent : AggregateRoot<CalendarEventId>
{
#pragma warning disable CS8618
    private CalendarEvent() { }
#pragma warning restore CS8618

    public static CalendarEvent Create(
        FamilyId familyId,
        UserId createdBy,
        EventTitle title,
        string? description,
        string? location,
        DateTime startTime,
        DateTime endTime,
        bool isAllDay)
    {
        var calendarEvent = new CalendarEvent
        {
            Id = CalendarEventId.New(),
            FamilyId = familyId,
            CreatedBy = createdBy,
            Title = title,
            Description = description,
            Location = location,
            StartTime = startTime,
            EndTime = endTime,
            IsAllDay = isAllDay,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        calendarEvent.RaiseDomainEvent(new CalendarEventCreatedEvent(
            calendarEvent.Id,
            calendarEvent.FamilyId,
            calendarEvent.CreatedBy,
            calendarEvent.Title,
            calendarEvent.StartTime,
            calendarEvent.EndTime,
            calendarEvent.CreatedAt
        ));

        return calendarEvent;
    }

    public void Update(
        EventTitle title,
        string? description,
        string? location,
        DateTime startTime,
        DateTime endTime,
        bool isAllDay)
    {
        Title = title;
        Description = description;
        Location = location;
        StartTime = startTime;
        EndTime = endTime;
        IsAllDay = isAllDay;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CalendarEventUpdatedEvent(
            Id,
            FamilyId,
            Title,
            StartTime,
            EndTime,
            UpdatedAt
        ));
    }

    public void Cancel()
    {
        if (IsCancelled)
        {
            throw new DomainException("Event is already cancelled", DomainErrorCodes.EventAlreadyCancelled);
        }

        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CalendarEventCancelledEvent(
            Id,
            FamilyId,
            UpdatedAt
        ));
    }

    public FamilyId FamilyId { get; private set; }
    public UserId CreatedBy { get; private set; }
    public EventTitle Title { get; private set; }
    public string? Description { get; private set; }
    public string? Location { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public bool IsAllDay { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<CalendarEventAttendee> Attendees { get; private set; } = new List<CalendarEventAttendee>();
}
