namespace FamilyHub.Api.Features.Dashboard.Models;

public class WidgetDescriptorDto
{
    public string WidgetTypeId { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DefaultWidth { get; set; }
    public int DefaultHeight { get; set; }
    public int MinWidth { get; set; }
    public int MinHeight { get; set; }
    public int MaxWidth { get; set; }
    public int MaxHeight { get; set; }
    public string? ConfigSchema { get; set; }
    public List<string> RequiredPermissions { get; set; } = [];
}
