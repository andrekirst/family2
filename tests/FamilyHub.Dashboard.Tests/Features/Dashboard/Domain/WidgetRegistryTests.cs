using FamilyHub.Api.Common.Widgets;
using FluentAssertions;

namespace FamilyHub.Dashboard.Tests.Features.Dashboard.Domain;

public class WidgetRegistryTests
{
    [Fact]
    public void RegisterProvider_ShouldAddWidgets()
    {
        // Arrange
        var registry = new WidgetRegistry();
        var provider = new TestWidgetProvider();

        // Act
        registry.RegisterProvider(provider);

        // Assert
        registry.GetAllWidgets().Should().HaveCount(2);
    }

    [Fact]
    public void GetAllWidgets_ShouldReturnAllRegistered()
    {
        // Arrange
        var registry = new WidgetRegistry();
        registry.RegisterProvider(new TestWidgetProvider());

        // Act
        var widgets = registry.GetAllWidgets();

        // Assert
        widgets.Should().HaveCount(2);
        widgets.Should().Contain(w => w.WidgetTypeId == "test:widget1");
        widgets.Should().Contain(w => w.WidgetTypeId == "test:widget2");
    }

    [Fact]
    public void GetWidgetsByModule_ShouldFilterByModule()
    {
        // Arrange
        var registry = new WidgetRegistry();
        registry.RegisterProvider(new TestWidgetProvider());
        registry.RegisterProvider(new AnotherWidgetProvider());

        // Act
        var testWidgets = registry.GetWidgetsByModule("test");
        var otherWidgets = registry.GetWidgetsByModule("other");

        // Assert
        testWidgets.Should().HaveCount(2);
        otherWidgets.Should().HaveCount(1);
    }

    [Fact]
    public void GetWidget_ShouldReturnMatchingWidget()
    {
        // Arrange
        var registry = new WidgetRegistry();
        registry.RegisterProvider(new TestWidgetProvider());

        // Act
        var widget = registry.GetWidget("test:widget1");

        // Assert
        widget.Should().NotBeNull();
        widget!.Name.Should().Be("Test Widget 1");
    }

    [Fact]
    public void GetWidget_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var registry = new WidgetRegistry();
        registry.RegisterProvider(new TestWidgetProvider());

        // Act
        var widget = registry.GetWidget("nonexistent:widget");

        // Assert
        widget.Should().BeNull();
    }

    [Fact]
    public void IsValidWidget_ShouldReturnTrue_ForRegistered()
    {
        // Arrange
        var registry = new WidgetRegistry();
        registry.RegisterProvider(new TestWidgetProvider());

        // Act & Assert
        registry.IsValidWidget("test:widget1").Should().BeTrue();
        registry.IsValidWidget("test:widget2").Should().BeTrue();
    }

    [Fact]
    public void IsValidWidget_ShouldReturnFalse_ForUnregistered()
    {
        // Arrange
        var registry = new WidgetRegistry();
        registry.RegisterProvider(new TestWidgetProvider());

        // Act & Assert
        registry.IsValidWidget("nonexistent:widget").Should().BeFalse();
    }

    private sealed class TestWidgetProvider : IWidgetProvider
    {
        public string ModuleName => "test";

        public IReadOnlyList<WidgetDescriptor> GetWidgets() =>
        [
            new WidgetDescriptor("test:widget1", "test", "Test Widget 1", "A test widget",
                6, 4, 2, 2, 12, 8, null, []),
            new WidgetDescriptor("test:widget2", "test", "Test Widget 2", "Another test widget",
                4, 3, 2, 2, 12, 8, null, ["test:permission"])
        ];
    }

    private sealed class AnotherWidgetProvider : IWidgetProvider
    {
        public string ModuleName => "other";

        public IReadOnlyList<WidgetDescriptor> GetWidgets() =>
        [
            new WidgetDescriptor("other:widget1", "other", "Other Widget", "A different widget",
                6, 4, 2, 2, 12, 8, null, [])
        ];
    }
}
