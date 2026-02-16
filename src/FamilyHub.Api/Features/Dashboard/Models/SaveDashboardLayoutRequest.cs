namespace FamilyHub.Api.Features.Dashboard.Models;

public class SaveDashboardLayoutRequest
{
    public required string Name { get; set; }
    public required bool IsShared { get; set; }
    public required List<WidgetPositionInput> Widgets { get; set; }
}

public class WidgetPositionInput
{
    public Guid? Id { get; set; }
    public required string WidgetType { get; set; }
    public required int X { get; set; }
    public required int Y { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public required int SortOrder { get; set; }
    public string? ConfigJson { get; set; }
}
