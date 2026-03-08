using FamilyHub.Common.Domain.ValueObjects;
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
        var (handler, repo) = CreateHandler();
        var layout = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("Test"), UserId.New());
        layout.ClearDomainEvents();
        repo.Seed(layout);

        var command = new AddWidgetCommand(
            layout.Id,
            WidgetTypeId.From("test:widget"),
            0, 0, 6, 4, null) { FamilyId = FamilyId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.WidgetType.Should().Be("test:widget");
        layout.Widgets.Should().HaveCount(1);
    }

    // Widget type validation and dashboard existence checks are now handled
    // by AddWidgetBusinessValidator and tested in validator tests

    private static (AddWidgetCommandHandler Handler, FakeDashboardLayoutRepository Repo)
        CreateHandler()
    {
        var repo = new FakeDashboardLayoutRepository();
        var handler = new AddWidgetCommandHandler(repo);
        return (handler, repo);
    }

}
