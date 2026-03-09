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

    public static DashboardLayout CreatePersonal(DashboardLayoutName name, UserId userId, DateTimeOffset utcNow)
    {
        var layout = new DashboardLayout
        {
            Id = DashboardId.New(),
            Name = name,
            UserId = userId,
            FamilyId = null,
            IsShared = false,
            CreatedAt = utcNow.UtcDateTime,
            UpdatedAt = utcNow.UtcDateTime
        };

        layout.RaiseDomainEvent(new DashboardCreatedEvent(
            layout.Id, userId, IsShared: false));

        return layout;
    }

    public static DashboardLayout CreateShared(DashboardLayoutName name, FamilyId familyId, UserId createdByUserId, DateTimeOffset utcNow)
    {
        var layout = new DashboardLayout
        {
            Id = DashboardId.New(),
            Name = name,
            UserId = null,
            FamilyId = familyId,
            IsShared = true,
            CreatedAt = utcNow.UtcDateTime,
            UpdatedAt = utcNow.UtcDateTime
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
        DateTimeOffset utcNow,
        string? configJson = null)
    {
        var widget = DashboardWidget.Create(Id, widgetType, x, y, width, height, sortOrder, utcNow, configJson);
        _widgets.Add(widget);
        UpdatedAt = utcNow.UtcDateTime;
        return widget;
    }

    public void RemoveWidget(DashboardWidgetId widgetId, DateTimeOffset utcNow)
    {
        var widget = _widgets.FirstOrDefault(w => w.Id == widgetId);
        if (widget is null)
        {
            throw new DomainException($"Widget {widgetId} not found in dashboard {Id}");
        }

        _widgets.Remove(widget);
        UpdatedAt = utcNow.UtcDateTime;
    }

    public void ReplaceAllWidgets(IReadOnlyList<DashboardWidget> newWidgets, DateTimeOffset utcNow)
    {
        _widgets.Clear();
        _widgets.AddRange(newWidgets);
        UpdatedAt = utcNow.UtcDateTime;
    }

    public void UpdateName(DashboardLayoutName name, DateTimeOffset utcNow)
    {
        Name = name;
        UpdatedAt = utcNow.UtcDateTime;
    }
}
