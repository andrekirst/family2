using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Application.Commands.AddWidget;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Dashboard.Tests.Features.Dashboard.Application;

public class AddWidgetCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAddWidget_WhenValid()
    {
        // Arrange
        var repo = Substitute.For<IDashboardLayoutRepository>();
        var handler = new AddWidgetCommandHandler(repo);

        var layout = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("Test"), UserId.New());
        layout.ClearDomainEvents();

        repo.GetByIdAsync(layout.Id, Arg.Any<CancellationToken>())
            .Returns(layout);

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
}
