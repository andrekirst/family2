using FamilyHub.Api.Features.FileManagement.Application.Commands.DisconnectExternalStorage;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DisconnectExternalStorageCommandHandlerTests
{
    private readonly IExternalConnectionRepository _repo = Substitute.For<IExternalConnectionRepository>();
    private readonly DisconnectExternalStorageCommandHandler _handler;

    public DisconnectExternalStorageCommandHandlerTests()
    {
        _handler = new DisconnectExternalStorageCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_ShouldDisconnectAndRemove()
    {
        var familyId = FamilyId.New();
        var connection = ExternalConnection.Create(
            familyId, ExternalProviderType.Dropbox, "Dropbox",
            "token", "refresh", DateTime.UtcNow.AddHours(1), UserId.New(), DateTimeOffset.UtcNow);
        _repo.GetByIdAsync(connection.Id, Arg.Any<CancellationToken>()).Returns(connection);

        var command = new DisconnectExternalStorageCommand(connection.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _repo.Received(1).RemoveAsync(connection, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrow()
    {
        _repo.GetByIdAsync(ExternalConnectionId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((ExternalConnection?)null);

        var command = new DisconnectExternalStorageCommand(ExternalConnectionId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*External connection not found*");
    }

    [Fact]
    public async Task Handle_WrongFamily_ShouldThrow()
    {
        var connection = ExternalConnection.Create(
            FamilyId.New(), ExternalProviderType.OneDrive, "OneDrive",
            "token", "refresh", DateTime.UtcNow.AddHours(1), UserId.New(), DateTimeOffset.UtcNow);
        _repo.GetByIdAsync(connection.Id, Arg.Any<CancellationToken>()).Returns(connection);

        var command = new DisconnectExternalStorageCommand(connection.Id)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*External connection not found*");
    }
}
