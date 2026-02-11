using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Domain.Entities;

public class CalendarEventAttendee
{
    public CalendarEventId CalendarEventId { get; set; }
    public UserId UserId { get; set; }

    public CalendarEvent CalendarEvent { get; set; } = null!;
}
