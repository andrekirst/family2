using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Widgets;
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
        var (handler, repo, _) = CreateHandler();
        var command = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("My Dashboard"),
            UserId.New(), null, false,
            [new WidgetPositionData(WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0, null)]);

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
        var (handler, repo, _) = CreateHandler();
        var userId = UserId.New();

        // Create initial dashboard
        var createCommand = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("My Dashboard"),
            userId, null, false,
            [new WidgetPositionData(WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0, null)]);
        await handler.Handle(createCommand, CancellationToken.None);

        // Update with new widgets
        var updateCommand = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("Updated Dashboard"),
            userId, null, false,
            [
                new WidgetPositionData(WidgetTypeId.From("test:widget"), 0, 0, 12, 2, 0, null),
                new WidgetPositionData(WidgetTypeId.From("test:widget2"), 0, 2, 6, 4, 1, null)
            ]);

        // Act
        var result = await handler.Handle(updateCommand, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        repo.UpdatedLayouts.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ShouldRejectInvalidWidgetType()
    {
        // Arrange
        var (handler, _, _) = CreateHandler();
        var command = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("My Dashboard"),
            UserId.New(), null, false,
            [new WidgetPositionData(WidgetTypeId.From("invalid:widget"), 0, 0, 6, 4, 0, null)]);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*invalid:widget*");
    }

    [Fact]
    public async Task Handle_ShouldCreateSharedDashboard()
    {
        // Arrange
        var (handler, repo, _) = CreateHandler();
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var command = new SaveDashboardLayoutCommand(
            DashboardLayoutName.From("Family Dashboard"),
            userId, familyId, true,
            [new WidgetPositionData(WidgetTypeId.From("test:widget"), 0, 0, 12, 2, 0, null)]);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        repo.AddedLayouts.Should().ContainSingle();
        repo.AddedLayouts[0].IsShared.Should().BeTrue();
        repo.AddedLayouts[0].FamilyId.Should().Be(familyId);
    }

    private static (SaveDashboardLayoutCommandHandler Handler, FakeDashboardLayoutRepository Repo, WidgetRegistry Registry)
        CreateHandler()
    {
        var repo = new FakeDashboardLayoutRepository();
        var registry = new WidgetRegistry();
        registry.RegisterProvider(new TestWidgetProvider());
        var handler = new SaveDashboardLayoutCommandHandler(repo, registry);
        return (handler, repo, registry);
    }

    private sealed class TestWidgetProvider : IWidgetProvider
    {
        public string ModuleName => "test";
        public IReadOnlyList<WidgetDescriptor> GetWidgets() =>
        [
            new WidgetDescriptor("test:widget", "test", "Test Widget", "A test widget",
                6, 4, 2, 2, 12, 8, null, []),
            new WidgetDescriptor("test:widget2", "test", "Test Widget 2", "Another test widget",
                4, 3, 2, 2, 12, 8, null, [])
        ];
    }
}
