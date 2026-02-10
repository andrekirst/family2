namespace FamilyHub.Api.Features.Calendar.Models;

public class CalendarEventDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public Guid CreatedBy { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAllDay { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CalendarEventAttendeeDto> Attendees { get; set; } = [];
}
