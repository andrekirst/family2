using FamilyHub.Api.Features.FileManagement.Application.Commands.ConnectExternalStorage;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class ConnectExternalStorageCommandHandlerTests
{
    private readonly IExternalConnectionRepository _repo = Substitute.For<IExternalConnectionRepository>();
    private readonly ConnectExternalStorageCommandHandler _handler;

    public ConnectExternalStorageCommandHandlerTests()
    {
        _handler = new ConnectExternalStorageCommandHandler(_repo, TimeProvider.System);
    }

    [Fact]
    public async Task Handle_ShouldCreateConnection()
    {
        var familyId = FamilyId.New();
        _repo.GetByFamilyAndProviderAsync(familyId, ExternalProviderType.OneDrive, Arg.Any<CancellationToken>())
            .Returns((ExternalConnection?)null);

        var command = new ConnectExternalStorageCommand(
            ExternalProviderType.OneDrive,
            "My OneDrive",
            "enc-access-token",
            "enc-refresh-token",
            DateTime.UtcNow.AddHours(1))
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.ConnectionId.Should().NotBe(Guid.Empty);
        await _repo.Received(1).AddAsync(
            Arg.Is<ExternalConnection>(c =>
                c.ProviderType == ExternalProviderType.OneDrive &&
                c.Status == ConnectionStatus.Connected),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateProvider_ShouldThrow()
    {
        var familyId = FamilyId.New();

        var existing = ExternalConnection.Create(
            familyId, ExternalProviderType.GoogleDrive, "Drive",
            "token", "refresh", DateTime.UtcNow.AddHours(1), UserId.New(), DateTimeOffset.UtcNow);
        _repo.GetByFamilyAndProviderAsync(familyId, ExternalProviderType.GoogleDrive, Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = new ConnectExternalStorageCommand(
            ExternalProviderType.GoogleDrive,
            "Another Drive",
            "new-token",
            "new-refresh",
            DateTime.UtcNow.AddHours(1))
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Match("*already exists*");
    }

    [Fact]
    public async Task Handle_PaperlessNgx_ShouldWorkWithoutRefreshToken()
    {
        var familyId = FamilyId.New();
        _repo.GetByFamilyAndProviderAsync(familyId, ExternalProviderType.PaperlessNgx, Arg.Any<CancellationToken>())
            .Returns((ExternalConnection?)null);

        var command = new ConnectExternalStorageCommand(
            ExternalProviderType.PaperlessNgx,
            "Paperless-ngx",
            "api-token-encrypted",
            null,
            null)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.ConnectionId.Should().NotBe(Guid.Empty);
        await _repo.Received(1).AddAsync(
            Arg.Is<ExternalConnection>(c =>
                c.EncryptedRefreshToken == null &&
                c.TokenExpiresAt == null),
            Arg.Any<CancellationToken>());
    }
}
