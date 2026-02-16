namespace FamilyHub.Api.Features.Dashboard.Models;

public class DashboardWidgetDto
{
    public Guid Id { get; set; }
    public string WidgetType { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int SortOrder { get; set; }
    public string? ConfigJson { get; set; }
}
