namespace FamilyHub.Api.Features.GoogleIntegration.Models;

public class GoogleCalendarSyncStatusDto
{
    public bool IsLinked { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public bool HasCalendarScope { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
