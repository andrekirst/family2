using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Events;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.Dashboard.Tests.Features.Dashboard.Domain;

public class DashboardLayoutTests
{
    [Fact]
    public void CreatePersonal_ShouldCreateWithValidData()
    {
        // Arrange
        var name = DashboardLayoutName.From("My Dashboard");
        var userId = UserId.New();

        // Act
        var layout = DashboardLayout.CreatePersonal(name, userId);

        // Assert
        layout.Should().NotBeNull();
        layout.Id.Value.Should().NotBe(Guid.Empty);
        layout.Name.Should().Be(name);
        layout.UserId.Should().Be(userId);
        layout.FamilyId.Should().BeNull();
        layout.IsShared.Should().BeFalse();
        layout.Widgets.Should().BeEmpty();
    }

    [Fact]
    public void CreatePersonal_ShouldRaiseDashboardCreatedEvent()
    {
        // Arrange
        var name = DashboardLayoutName.From("My Dashboard");
        var userId = UserId.New();

        // Act
        var layout = DashboardLayout.CreatePersonal(name, userId);

        // Assert
        layout.DomainEvents.Should().HaveCount(1);
        var domainEvent = layout.DomainEvents.First();
        domainEvent.Should().BeOfType<DashboardCreatedEvent>();

        var evt = (DashboardCreatedEvent)domainEvent;
        evt.DashboardId.Should().Be(layout.Id);
        evt.CreatedByUserId.Should().Be(userId);
        evt.IsShared.Should().BeFalse();
    }

    [Fact]
    public void CreateShared_ShouldCreateWithValidData()
    {
        // Arrange
        var name = DashboardLayoutName.From("Family Dashboard");
        var familyId = FamilyId.New();
        var createdBy = UserId.New();

        // Act
        var layout = DashboardLayout.CreateShared(name, familyId, createdBy);

        // Assert
        layout.Should().NotBeNull();
        layout.UserId.Should().BeNull();
        layout.FamilyId.Should().Be(familyId);
        layout.IsShared.Should().BeTrue();
    }

    [Fact]
    public void CreateShared_ShouldRaiseDashboardCreatedEvent()
    {
        // Arrange
        var name = DashboardLayoutName.From("Family Dashboard");
        var familyId = FamilyId.New();
        var createdBy = UserId.New();

        // Act
        var layout = DashboardLayout.CreateShared(name, familyId, createdBy);

        // Assert
        var evt = layout.DomainEvents.OfType<DashboardCreatedEvent>().Single();
        evt.IsShared.Should().BeTrue();
        evt.CreatedByUserId.Should().Be(createdBy);
    }

    [Fact]
    public void AddWidget_ShouldAddWidgetToLayout()
    {
        // Arrange
        var layout = CreateTestLayout();
        var widgetType = WidgetTypeId.From("dashboard:welcome");

        // Act
        var widget = layout.AddWidget(widgetType, 0, 0, 12, 2, 0);

        // Assert
        layout.Widgets.Should().HaveCount(1);
        widget.WidgetType.Should().Be(widgetType);
        widget.X.Should().Be(0);
        widget.Y.Should().Be(0);
        widget.Width.Should().Be(12);
        widget.Height.Should().Be(2);
    }

    [Fact]
    public void RemoveWidget_ShouldRemoveExistingWidget()
    {
        // Arrange
        var layout = CreateTestLayout();
        var widget = layout.AddWidget(WidgetTypeId.From("dashboard:welcome"), 0, 0, 12, 2, 0);

        // Act
        layout.RemoveWidget(widget.Id);

        // Assert
        layout.Widgets.Should().BeEmpty();
    }

    [Fact]
    public void RemoveWidget_ShouldThrowForMissingWidget()
    {
        // Arrange
        var layout = CreateTestLayout();
        var nonExistentId = DashboardWidgetId.New();

        // Act
        var act = () => layout.RemoveWidget(nonExistentId);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ReplaceAllWidgets_ShouldClearAndAddNewWidgets()
    {
        // Arrange
        var layout = CreateTestLayout();
        layout.AddWidget(WidgetTypeId.From("old:widget"), 0, 0, 6, 4, 0);
        layout.AddWidget(WidgetTypeId.From("old:widget2"), 6, 0, 6, 4, 1);

        var newWidgets = new List<DashboardWidget>
        {
            DashboardWidget.Create(layout.Id, WidgetTypeId.From("new:widget"), 0, 0, 12, 2, 0)
        };

        // Act
        layout.ReplaceAllWidgets(newWidgets);

        // Assert
        layout.Widgets.Should().HaveCount(1);
        layout.Widgets.First().WidgetType.Value.Should().Be("new:widget");
    }

    [Fact]
    public void ReplaceAllWidgets_WithEmptyList_ShouldClearWidgets()
    {
        // Arrange
        var layout = CreateTestLayout();
        layout.AddWidget(WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0);

        // Act
        layout.ReplaceAllWidgets([]);

        // Assert
        layout.Widgets.Should().BeEmpty();
    }

    [Fact]
    public void CreatePersonal_ShouldGenerateUniqueIds()
    {
        // Arrange
        var name = DashboardLayoutName.From("My Dashboard");
        var userId = UserId.New();

        // Act
        var layout1 = DashboardLayout.CreatePersonal(name, userId);
        var layout2 = DashboardLayout.CreatePersonal(name, userId);

        // Assert
        layout1.Id.Should().NotBe(layout2.Id);
    }

    private static DashboardLayout CreateTestLayout()
    {
        var layout = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("Test Dashboard"),
            UserId.New());
        layout.ClearDomainEvents();
        return layout;
    }
}
