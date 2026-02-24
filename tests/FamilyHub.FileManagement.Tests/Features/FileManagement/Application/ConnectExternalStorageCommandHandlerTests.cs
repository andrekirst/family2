using FamilyHub.Api.Features.FileManagement.Application.Commands.ConnectExternalStorage;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class ConnectExternalStorageCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateConnection()
    {
        var repo = new FakeExternalConnectionRepository();
        var handler = new ConnectExternalStorageCommandHandler(repo);

        var command = new ConnectExternalStorageCommand(
            FamilyId.New(),
            ExternalProviderType.OneDrive,
            "My OneDrive",
            "enc-access-token",
            "enc-refresh-token",
            DateTime.UtcNow.AddHours(1),
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.ConnectionId.Should().NotBe(Guid.Empty);
        repo.Connections.Should().HaveCount(1);
        repo.Connections.First().ProviderType.Should().Be(ExternalProviderType.OneDrive);
        repo.Connections.First().Status.Should().Be(ConnectionStatus.Connected);
    }

    [Fact]
    public async Task Handle_DuplicateProvider_ShouldThrow()
    {
        var repo = new FakeExternalConnectionRepository();
        var handler = new ConnectExternalStorageCommandHandler(repo);
        var familyId = FamilyId.New();

        var existing = ExternalConnection.Create(
            familyId, ExternalProviderType.GoogleDrive, "Drive",
            "token", "refresh", DateTime.UtcNow.AddHours(1), UserId.New());
        repo.Connections.Add(existing);

        var command = new ConnectExternalStorageCommand(
            familyId,
            ExternalProviderType.GoogleDrive,
            "Another Drive",
            "new-token",
            "new-refresh",
            DateTime.UtcNow.AddHours(1),
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_PaperlessNgx_ShouldWorkWithoutRefreshToken()
    {
        var repo = new FakeExternalConnectionRepository();
        var handler = new ConnectExternalStorageCommandHandler(repo);

        var command = new ConnectExternalStorageCommand(
            FamilyId.New(),
            ExternalProviderType.PaperlessNgx,
            "Paperless-ngx",
            "api-token-encrypted",
            null,
            null,
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.ConnectionId.Should().NotBe(Guid.Empty);
        var conn = repo.Connections.First();
        conn.EncryptedRefreshToken.Should().BeNull();
        conn.TokenExpiresAt.Should().BeNull();
    }
}
