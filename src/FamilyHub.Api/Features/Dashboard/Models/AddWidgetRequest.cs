namespace FamilyHub.Api.Features.Dashboard.Models;

public class AddWidgetRequest
{
    public required Guid DashboardId { get; set; }
    public required string WidgetType { get; set; }
    public required int X { get; set; }
    public required int Y { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public string? ConfigJson { get; set; }
}
