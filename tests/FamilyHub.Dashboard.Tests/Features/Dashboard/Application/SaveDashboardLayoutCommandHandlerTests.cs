using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Application.Commands.SaveDashboardLayout;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Dashboard.Tests.Features.Dashboard.Application;

public class SaveDashboardLayoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateNewDashboard_WhenNoneExists()
    {
        // Arrange
        var repo = Substitute.For<IDashboardLayoutRepository>();
        var handler = new SaveDashboardLayoutCommandHandler(repo);
        var userId = UserId.New();

        repo.GetPersonalDashboardAsync(userId, Arg.Any<CancellationToken>())
            .Returns((DashboardLayout?)null);

        var command = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("My Dashboard"),
            false,
            [new WidgetPositionData(WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0, null)]) { UserId = userId, FamilyId = FamilyId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        await repo.Received(1).AddAsync(
            Arg.Is<DashboardLayout>(d => d.Widgets.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingDashboard()
    {
        // Arrange
        var repo = Substitute.For<IDashboardLayoutRepository>();
        var handler = new SaveDashboardLayoutCommandHandler(repo);
        var userId = UserId.New();
        var familyId = FamilyId.New();

        // First call: no existing dashboard -> creates new
        repo.GetPersonalDashboardAsync(userId, Arg.Any<CancellationToken>())
            .Returns((DashboardLayout?)null);

        var createCommand = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("My Dashboard"),
            false,
            [new WidgetPositionData(WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0, null)]) { UserId = userId, FamilyId = familyId };
        var createResult = await handler.Handle(createCommand, CancellationToken.None);

        // Second call: existing dashboard -> updates
        repo.GetPersonalDashboardAsync(userId, Arg.Any<CancellationToken>())
            .Returns(createResult.Layout);

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
        await repo.Received(1).UpdateAsync(
            Arg.Any<DashboardLayout>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateSharedDashboard()
    {
        // Arrange
        var repo = Substitute.For<IDashboardLayoutRepository>();
        var handler = new SaveDashboardLayoutCommandHandler(repo);
        var familyId = FamilyId.New();
        var userId = UserId.New();

        repo.GetSharedDashboardAsync(familyId, Arg.Any<CancellationToken>())
            .Returns((DashboardLayout?)null);

        var command = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("Family Dashboard"),
            true,
            [new WidgetPositionData(WidgetTypeId.From("test:widget"), 0, 0, 12, 2, 0, null)]) { UserId = userId, FamilyId = familyId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        await repo.Received(1).AddAsync(
            Arg.Is<DashboardLayout>(d => d.IsShared && d.FamilyId == familyId),
            Arg.Any<CancellationToken>());
    }
}
