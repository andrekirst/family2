namespace FamilyHub.Api.Features.Calendar.Models;

public class CreateCalendarEventRequest
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAllDay { get; set; }
    public List<Guid> AttendeeIds { get; set; } = [];
}
