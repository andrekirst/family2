using FluentAssertions;
using FamilyHub.Api.Features.ModuleName.Application.Commands.CommandName;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.ModuleName.Tests.Features.ModuleName.Application;

public class CommandNameCommandHandlerTests
{
    private static CommandNameCommand CreateCommand(FamilyId? familyId = null)
    {
        return new CommandNameCommand(
            FamilyId: familyId ?? FamilyId.From(Guid.NewGuid()));
    }

    [Fact]
    public async Task Handle_should_return_success()
    {
        // Arrange
        var handler = new CommandNameCommandHandler();
        var command = CreateCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
    }
}
