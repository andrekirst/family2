using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.Events;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Domain.Entities;

public sealed class DashboardLayout : AggregateRoot<DashboardId>
{
#pragma warning disable CS8618
    private DashboardLayout() { }
#pragma warning restore CS8618

    private readonly List<DashboardWidget> _widgets = [];

    public static DashboardLayout CreatePersonal(DashboardLayoutName name, UserId userId)
    {
        var layout = new DashboardLayout
        {
            Id = DashboardId.New(),
            Name = name,
            UserId = userId,
            FamilyId = null,
            IsShared = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        layout.RaiseDomainEvent(new DashboardCreatedEvent(
            layout.Id, userId, IsShared: false));

        return layout;
    }

    public static DashboardLayout CreateShared(DashboardLayoutName name, FamilyId familyId, UserId createdByUserId)
    {
        var layout = new DashboardLayout
        {
            Id = DashboardId.New(),
            Name = name,
            UserId = null,
            FamilyId = familyId,
            IsShared = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        layout.RaiseDomainEvent(new DashboardCreatedEvent(
            layout.Id, createdByUserId, IsShared: true));

        return layout;
    }

    public DashboardLayoutName Name { get; private set; }
    public UserId? UserId { get; private set; }
    public FamilyId? FamilyId { get; private set; }
    public bool IsShared { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DashboardWidget> Widgets => _widgets.AsReadOnly();

    public DashboardWidget AddWidget(
        WidgetTypeId widgetType,
        int x, int y,
        int width, int height,
        int sortOrder,
        string? configJson = null)
    {
        var widget = DashboardWidget.Create(Id, widgetType, x, y, width, height, sortOrder, configJson);
        _widgets.Add(widget);
        UpdatedAt = DateTime.UtcNow;
        return widget;
    }

    public void RemoveWidget(DashboardWidgetId widgetId)
    {
        var widget = _widgets.FirstOrDefault(w => w.Id == widgetId);
        if (widget is null)
            throw new DomainException($"Widget {widgetId} not found in dashboard {Id}");

        _widgets.Remove(widget);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceAllWidgets(IReadOnlyList<DashboardWidget> newWidgets)
    {
        _widgets.Clear();
        _widgets.AddRange(newWidgets);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(DashboardLayoutName name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}
