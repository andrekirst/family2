using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Application.Commands.UpdateWidgetConfig;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Dashboard.Tests.Features.Dashboard.Application;

public class UpdateWidgetConfigCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateConfig()
    {
        // Arrange
        var repo = Substitute.For<IDashboardLayoutRepository>();
        var handler = new UpdateWidgetConfigCommandHandler(repo, TimeProvider.System);

        var layout = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("Test"), UserId.New(), DateTimeOffset.UtcNow);
        var widget = layout.AddWidget(WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0, DateTimeOffset.UtcNow);
        layout.ClearDomainEvents();

        repo.GetByWidgetIdAsync(widget.Id, Arg.Any<CancellationToken>())
            .Returns(layout);

        var newConfig = """{"showCount": 5}""";

        // Act
        var result = await handler.Handle(
            new UpdateWidgetConfigCommand(widget.Id, newConfig) { FamilyId = FamilyId.New() }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConfigJson.Should().Be(newConfig);
    }

    [Fact]
    public async Task Handle_ShouldClearConfig_WhenNull()
    {
        // Arrange
        var repo = Substitute.For<IDashboardLayoutRepository>();
        var handler = new UpdateWidgetConfigCommandHandler(repo, TimeProvider.System);

        var layout = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("Test"), UserId.New(), DateTimeOffset.UtcNow);
        var widget = layout.AddWidget(
            WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0, DateTimeOffset.UtcNow, """{"old": true}""");
        layout.ClearDomainEvents();

        repo.GetByWidgetIdAsync(widget.Id, Arg.Any<CancellationToken>())
            .Returns(layout);

        // Act
        var result = await handler.Handle(
            new UpdateWidgetConfigCommand(widget.Id, null) { FamilyId = FamilyId.New() }, CancellationToken.None);

        // Assert
        result.ConfigJson.Should().BeNull();
    }
}
