using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Application.Commands.RemoveWidget;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Dashboard.Tests.Features.Dashboard.Application;

public class RemoveWidgetCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemoveWidget_WhenExists()
    {
        // Arrange
        var repo = Substitute.For<IDashboardLayoutRepository>();
        var handler = new RemoveWidgetCommandHandler(repo, TimeProvider.System);

        var layout = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("Test"), UserId.New(), DateTimeOffset.UtcNow);
        var widget = layout.AddWidget(WidgetTypeId.From("test:widget"), 0, 0, 6, 4, 0, DateTimeOffset.UtcNow);
        layout.ClearDomainEvents();

        repo.GetByWidgetIdAsync(widget.Id, Arg.Any<CancellationToken>())
            .Returns(layout);

        // Act
        var result = await handler.Handle(
            new RemoveWidgetCommand(widget.Id) { FamilyId = FamilyId.New() }, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        layout.Widgets.Should().BeEmpty();
    }
}
