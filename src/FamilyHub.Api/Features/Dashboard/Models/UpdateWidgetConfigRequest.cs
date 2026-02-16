namespace FamilyHub.Api.Features.Dashboard.Models;

public class UpdateWidgetConfigRequest
{
    public required Guid WidgetId { get; set; }
    public string? ConfigJson { get; set; }
}
