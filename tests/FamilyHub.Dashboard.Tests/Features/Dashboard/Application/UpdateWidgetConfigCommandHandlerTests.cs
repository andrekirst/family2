using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Application.Commands.UpdateWidgetConfig;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Dashboard.Tests.Features.Dashboard.Application;

public class UpdateWidgetConfigCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateConfig()
    {
        // Arrange
        var repo = new FakeDashboardLayoutRepository();
        var handler = new UpdateWidgetConfigCommandHandler(repo);

        var layout = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("Test"), UserId.New());
        var widget = layout.AddWidget(WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0);
        layout.ClearDomainEvents();
        repo.Seed(layout);

        var newConfig = """{"showCount": 5}""";

        // Act
        var result = await handler.Handle(
            new UpdateWidgetConfigCommand(widget.Id, newConfig), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConfigJson.Should().Be(newConfig);
    }

    [Fact]
    public async Task Handle_ShouldClearConfig_WhenNull()
    {
        // Arrange
        var repo = new FakeDashboardLayoutRepository();
        var handler = new UpdateWidgetConfigCommandHandler(repo);

        var layout = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("Test"), UserId.New());
        var widget = layout.AddWidget(
            WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0, """{"old": true}""");
        layout.ClearDomainEvents();
        repo.Seed(layout);

        // Act
        var result = await handler.Handle(
            new UpdateWidgetConfigCommand(widget.Id, null), CancellationToken.None);

        // Assert
        result.ConfigJson.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWidgetNotFound()
    {
        // Arrange
        var repo = new FakeDashboardLayoutRepository();
        var handler = new UpdateWidgetConfigCommandHandler(repo);

        // Act
        var act = async () => await handler.Handle(
            new UpdateWidgetConfigCommand(DashboardWidgetId.New(), "{}"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }
}
