using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Widgets;
using FamilyHub.Api.Features.Dashboard.Application.Commands.AddWidget;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Dashboard.Tests.Features.Dashboard.Application;

public class AddWidgetCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAddWidget_WhenValid()
    {
        // Arrange
        var (handler, repo, _) = CreateHandler();
        var layout = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("Test"), UserId.New());
        layout.ClearDomainEvents();
        repo.Seed(layout);

        var command = new AddWidgetCommand(
            layout.Id,
            WidgetTypeId.From("test:widget"),
            0, 0, 6, 4, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.WidgetType.Should().Be("test:widget");
        layout.Widgets.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldRejectInvalidWidgetType()
    {
        // Arrange
        var (handler, repo, _) = CreateHandler();
        var layout = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("Test"), UserId.New());
        layout.ClearDomainEvents();
        repo.Seed(layout);

        var command = new AddWidgetCommand(
            layout.Id,
            WidgetTypeId.From("invalid:widget"),
            0, 0, 6, 4, null);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenDashboardNotFound()
    {
        // Arrange
        var (handler, _, _) = CreateHandler();
        var command = new AddWidgetCommand(
            DashboardId.New(),
            WidgetTypeId.From("test:widget"),
            0, 0, 6, 4, null);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*not found*");
    }

    private static (AddWidgetCommandHandler Handler, FakeDashboardLayoutRepository Repo, WidgetRegistry Registry)
        CreateHandler()
    {
        var repo = new FakeDashboardLayoutRepository();
        var registry = new WidgetRegistry();
        registry.RegisterProvider(new TestWidgetProvider());
        var handler = new AddWidgetCommandHandler(repo, registry);
        return (handler, repo, registry);
    }

    private sealed class TestWidgetProvider : IWidgetProvider
    {
        public string ModuleName => "test";
        public IReadOnlyList<WidgetDescriptor> GetWidgets() =>
        [
            new WidgetDescriptor("test:widget", "test", "Test Widget", "A test widget",
                6, 4, 2, 2, 12, 8, null, [])
        ];
    }
}
