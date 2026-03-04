using FamilyHub.Api.Common.Search;
using FluentAssertions;

namespace FamilyHub.Search.Tests;

public class CommandPaletteRegistryTests
{
    [Fact]
    public void RegisterProvider_ShouldAddCommands()
    {
        // Arrange
        var registry = new CommandPaletteRegistry();
        var provider = new TestCommandProvider("family");

        // Act
        registry.RegisterProvider(provider);

        // Assert
        registry.GetAllCommands().Should().HaveCount(2);
    }

    [Fact]
    public void GetAllCommands_ShouldReturnAllRegistered()
    {
        // Arrange
        var registry = new CommandPaletteRegistry();
        registry.RegisterProvider(new TestCommandProvider("family"));

        // Act
        var commands = registry.GetAllCommands();

        // Assert
        commands.Should().HaveCount(2);
        commands.Should().Contain(c => c.Label == "Command 1");
        commands.Should().Contain(c => c.Label == "Command 2");
    }

    [Fact]
    public void GetCommandsByModule_ShouldFilterByGroup()
    {
        // Arrange
        var registry = new CommandPaletteRegistry();
        registry.RegisterProvider(new TestCommandProvider("family"));
        registry.RegisterProvider(new TestCommandProvider("calendar"));

        // Act
        var familyCommands = registry.GetCommandsByModule("family");
        var calendarCommands = registry.GetCommandsByModule("calendar");

        // Assert
        familyCommands.Should().HaveCount(2);
        calendarCommands.Should().HaveCount(2);
    }

    [Fact]
    public void GetAllCommands_MultipleProviders_ShouldCombine()
    {
        // Arrange
        var registry = new CommandPaletteRegistry();
        registry.RegisterProvider(new TestCommandProvider("family"));
        registry.RegisterProvider(new TestCommandProvider("calendar"));

        // Act
        var commands = registry.GetAllCommands();

        // Assert
        commands.Should().HaveCount(4);
    }

    [Fact]
    public void GetAllCommands_NoProviders_ShouldReturnEmpty()
    {
        // Arrange
        var registry = new CommandPaletteRegistry();

        // Act
        var commands = registry.GetAllCommands();

        // Assert
        commands.Should().BeEmpty();
    }

    private sealed class TestCommandProvider(string group) : ICommandPaletteProvider
    {
        public string ModuleName => group;

        public IReadOnlyList<CommandDescriptor> GetCommands() =>
        [
            new CommandDescriptor("Command 1", "Description 1", ["key1"], "/route1", [], "icon1", group),
            new CommandDescriptor("Command 2", "Description 2", ["key2"], "/route2", ["perm:test"], "icon2", group)
        ];
    }
}
