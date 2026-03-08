using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Application.Commands.RemoveWidget;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Dashboard.Tests.Features.Dashboard.Application;

public class RemoveWidgetCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemoveWidget_WhenExists()
    {
        // Arrange
        var repo = new FakeDashboardLayoutRepository();
        var handler = new RemoveWidgetCommandHandler(repo);

        var layout = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("Test"), UserId.New());
        var widget = layout.AddWidget(WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0);
        layout.ClearDomainEvents();
        repo.Seed(layout);

        // Act
        var result = await handler.Handle(
            new RemoveWidgetCommand(widget.Id) { FamilyId = FamilyId.New() }, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        layout.Widgets.Should().BeEmpty();
    }
}
