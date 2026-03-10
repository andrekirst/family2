using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Domain.Entities;

public sealed class DashboardWidget
{
#pragma warning disable CS8618
    private DashboardWidget() { }
#pragma warning restore CS8618

    public static DashboardWidget Create(
        DashboardId dashboardId,
        WidgetTypeId widgetType,
        int x, int y,
        int width, int height,
        int sortOrder,
        DateTimeOffset utcNow,
        string? configJson = null)
    {
        return new DashboardWidget
        {
            Id = DashboardWidgetId.New(),
            DashboardId = dashboardId,
            WidgetType = widgetType,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            SortOrder = sortOrder,
            ConfigJson = configJson,
            CreatedAt = utcNow.UtcDateTime,
            UpdatedAt = utcNow.UtcDateTime
        };
    }

    public DashboardWidgetId Id { get; private set; }
    public DashboardId DashboardId { get; private set; }
    public WidgetTypeId WidgetType { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int SortOrder { get; private set; }
    public string? ConfigJson { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void UpdatePosition(int x, int y, int width, int height, int sortOrder, DateTimeOffset utcNow)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        SortOrder = sortOrder;
        UpdatedAt = utcNow.UtcDateTime;
    }

    public void UpdateConfig(string? configJson, DateTimeOffset utcNow)
    {
        ConfigJson = configJson;
        UpdatedAt = utcNow.UtcDateTime;
    }
}
