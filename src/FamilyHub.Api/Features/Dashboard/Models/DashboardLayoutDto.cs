namespace FamilyHub.Api.Features.Dashboard.Models;

public class DashboardLayoutDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<DashboardWidgetDto> Widgets { get; set; } = [];
}
