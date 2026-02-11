namespace FamilyHub.Api.Features.Calendar.Models;

public class CalendarCleanupOptions
{
    public const string SectionName = "Calendar:Cleanup";
    public int RetentionDays { get; set; } = 30;
    public int IntervalHours { get; set; } = 24;
}
