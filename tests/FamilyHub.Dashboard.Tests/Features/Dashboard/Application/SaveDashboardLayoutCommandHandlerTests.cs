using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Application.Commands.SaveDashboardLayout;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Dashboard.Tests.Features.Dashboard.Application;

public class SaveDashboardLayoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateNewDashboard_WhenNoneExists()
    {
        // Arrange
        var (handler, repo) = CreateHandler();
        var command = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("My Dashboard"),
            false,
            [new WidgetPositionData(WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0, null)]) { UserId = UserId.New(), FamilyId = FamilyId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        repo.AddedLayouts.Should().ContainSingle();
        repo.AddedLayouts[0].Widgets.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingDashboard()
    {
        // Arrange
        var (handler, repo) = CreateHandler();
        var userId = UserId.New();

        // Create initial dashboard
        var familyId = FamilyId.New();
        var createCommand = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("My Dashboard"),
            false,
            [new WidgetPositionData(WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0, null)]) { UserId = userId, FamilyId = familyId };
        await handler.Handle(createCommand, CancellationToken.None);

        // Update with new widgets
        var updateCommand = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("Updated Dashboard"),
            false,
            [
                new WidgetPositionData(WidgetTypeId.From("test:widget"), 0, 0, 12, 2, 0, null),
                new WidgetPositionData(WidgetTypeId.From("test:widget2"), 0, 2, 6, 4, 1, null)
            ]) { UserId = userId, FamilyId = familyId };

        // Act
        var result = await handler.Handle(updateCommand, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        repo.UpdatedLayouts.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ShouldCreateSharedDashboard()
    {
        // Arrange
        var (handler, repo) = CreateHandler();
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var command = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("Family Dashboard"),
            true,
            [new WidgetPositionData(WidgetTypeId.From("test:widget"), 0, 0, 12, 2, 0, null)]) { UserId = userId, FamilyId = familyId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        repo.AddedLayouts.Should().ContainSingle();
        repo.AddedLayouts[0].IsShared.Should().BeTrue();
        repo.AddedLayouts[0].FamilyId.Should().Be(familyId);
    }

    private static (SaveDashboardLayoutCommandHandler Handler, FakeDashboardLayoutRepository Repo)
        CreateHandler()
    {
        var repo = new FakeDashboardLayoutRepository();
        var handler = new SaveDashboardLayoutCommandHandler(repo);
        return (handler, repo);
    }
}
