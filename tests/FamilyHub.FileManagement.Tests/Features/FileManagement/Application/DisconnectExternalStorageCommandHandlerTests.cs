using FamilyHub.Api.Features.FileManagement.Application.Commands.DisconnectExternalStorage;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DisconnectExternalStorageCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDisconnectAndRemove()
    {
        var repo = new FakeExternalConnectionRepository();
        var handler = new DisconnectExternalStorageCommandHandler(repo);
        var familyId = FamilyId.New();

        var connection = ExternalConnection.Create(
            familyId, ExternalProviderType.Dropbox, "Dropbox",
            "token", "refresh", DateTime.UtcNow.AddHours(1), UserId.New());
        repo.Connections.Add(connection);

        var command = new DisconnectExternalStorageCommand(connection.Id, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        repo.Connections.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrow()
    {
        var repo = new FakeExternalConnectionRepository();
        var handler = new DisconnectExternalStorageCommandHandler(repo);

        var command = new DisconnectExternalStorageCommand(
            ExternalConnectionId.New(), FamilyId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*External connection not found*");
    }

    [Fact]
    public async Task Handle_WrongFamily_ShouldThrow()
    {
        var repo = new FakeExternalConnectionRepository();
        var handler = new DisconnectExternalStorageCommandHandler(repo);

        var connection = ExternalConnection.Create(
            FamilyId.New(), ExternalProviderType.OneDrive, "OneDrive",
            "token", "refresh", DateTime.UtcNow.AddHours(1), UserId.New());
        repo.Connections.Add(connection);

        var command = new DisconnectExternalStorageCommand(connection.Id, FamilyId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*External connection not found*");
    }
}
