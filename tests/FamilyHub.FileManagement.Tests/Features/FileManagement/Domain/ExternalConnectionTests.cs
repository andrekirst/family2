using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain;

public class ExternalConnectionTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var connection = ExternalConnection.Create(
            familyId, ExternalProviderType.OneDrive, "My OneDrive",
            "enc-token", "enc-refresh", DateTime.UtcNow.AddHours(1), userId);

        connection.FamilyId.Should().Be(familyId);
        connection.ProviderType.Should().Be(ExternalProviderType.OneDrive);
        connection.DisplayName.Should().Be("My OneDrive");
        connection.EncryptedAccessToken.Should().Be("enc-token");
        connection.EncryptedRefreshToken.Should().Be("enc-refresh");
        connection.ConnectedBy.Should().Be(userId);
        connection.Status.Should().Be(ConnectionStatus.Connected);
    }

    [Fact]
    public void Create_ShouldRaiseExternalStorageConnectedEvent()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var connection = ExternalConnection.Create(
            familyId, ExternalProviderType.GoogleDrive, "My Drive",
            "token", null, null, userId);

        var domainEvent = connection.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ExternalStorageConnectedEvent>().Subject;
        domainEvent.ProviderType.Should().Be(ExternalProviderType.GoogleDrive);
        domainEvent.FamilyId.Should().Be(familyId);
        domainEvent.ConnectedBy.Should().Be(userId);
    }

    [Fact]
    public void Disconnect_ShouldSetStatusAndRaiseEvent()
    {
        var connection = ExternalConnection.Create(
            FamilyId.New(), ExternalProviderType.Dropbox, "Dropbox",
            "token", "refresh", DateTime.UtcNow.AddHours(1), UserId.New());

        connection.ClearDomainEvents();
        connection.Disconnect();

        connection.Status.Should().Be(ConnectionStatus.Disconnected);
        var domainEvent = connection.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ExternalStorageDisconnectedEvent>().Subject;
        domainEvent.ProviderType.Should().Be(ExternalProviderType.Dropbox);
    }

    [Fact]
    public void UpdateTokens_ShouldRefreshTokensAndStatus()
    {
        var connection = ExternalConnection.Create(
            FamilyId.New(), ExternalProviderType.OneDrive, "OneDrive",
            "old-token", "old-refresh", DateTime.UtcNow.AddMinutes(-5), UserId.New());

        connection.MarkExpired();
        connection.Status.Should().Be(ConnectionStatus.Expired);

        connection.UpdateTokens("new-token", "new-refresh", DateTime.UtcNow.AddHours(1));

        connection.EncryptedAccessToken.Should().Be("new-token");
        connection.EncryptedRefreshToken.Should().Be("new-refresh");
        connection.Status.Should().Be(ConnectionStatus.Connected);
    }

    [Fact]
    public void IsTokenExpired_ShouldReturnTrueWhenPastExpiry()
    {
        var connection = ExternalConnection.Create(
            FamilyId.New(), ExternalProviderType.GoogleDrive, "Drive",
            "token", "refresh", DateTime.UtcNow.AddMinutes(-1), UserId.New());

        connection.IsTokenExpired.Should().BeTrue();
    }

    [Fact]
    public void IsTokenExpired_ShouldReturnFalseWhenNotExpired()
    {
        var connection = ExternalConnection.Create(
            FamilyId.New(), ExternalProviderType.GoogleDrive, "Drive",
            "token", "refresh", DateTime.UtcNow.AddHours(1), UserId.New());

        connection.IsTokenExpired.Should().BeFalse();
    }

    [Fact]
    public void IsTokenExpired_ShouldReturnFalseWhenNoExpiry()
    {
        var connection = ExternalConnection.Create(
            FamilyId.New(), ExternalProviderType.PaperlessNgx, "Paperless",
            "api-token", null, null, UserId.New());

        connection.IsTokenExpired.Should().BeFalse();
    }

    [Fact]
    public void MarkError_ShouldSetErrorStatus()
    {
        var connection = ExternalConnection.Create(
            FamilyId.New(), ExternalProviderType.Dropbox, "Dropbox",
            "token", "refresh", DateTime.UtcNow.AddHours(1), UserId.New());

        connection.MarkError();

        connection.Status.Should().Be(ConnectionStatus.Error);
    }
}
