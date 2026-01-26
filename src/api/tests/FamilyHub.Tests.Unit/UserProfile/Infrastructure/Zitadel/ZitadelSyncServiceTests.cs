using FamilyHub.Modules.UserProfile.Application.Abstractions;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.Modules.UserProfile.Infrastructure.Zitadel;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.Tests.Unit.UserProfile.Infrastructure.Zitadel;

/// <summary>
/// Unit tests for the ZitadelSyncService.
/// Tests bidirectional sync functionality between Family Hub and Zitadel.
/// </summary>
public class ZitadelSyncServiceTests
{
    private readonly IZitadelManagementApiClient _zitadelClient;
    private readonly IUserLookupService _userLookupService;
    private readonly ILogger<ZitadelSyncService> _logger;
    private readonly ZitadelSyncService _sut;

    public ZitadelSyncServiceTests()
    {
        _zitadelClient = Substitute.For<IZitadelManagementApiClient>();
        _userLookupService = Substitute.For<IUserLookupService>();
        _logger = Substitute.For<ILogger<ZitadelSyncService>>();
        _sut = new ZitadelSyncService(_zitadelClient, _userLookupService, _logger);
    }

    #region PushDisplayNameAsync Tests

    [Fact]
    public async Task PushDisplayNameAsync_WithValidUser_PushesToZitadel()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("John Doe");
        var externalUserId = "zitadel-user-123";

        _userLookupService.GetExternalUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(externalUserId);
        _zitadelClient.UpdateUserProfileAsync(externalUserId, displayName.Value, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.PushDisplayNameAsync(userId, displayName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _zitadelClient.Received(1)
            .UpdateUserProfileAsync(externalUserId, displayName.Value, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PushDisplayNameAsync_WhenExternalUserIdNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("John Doe");

        _userLookupService.GetExternalUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        // Act
        var result = await _sut.PushDisplayNameAsync(userId, displayName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("External user ID not found");
        await _zitadelClient.DidNotReceive()
            .UpdateUserProfileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PushDisplayNameAsync_WhenZitadelUpdateFails_ReturnsFailure()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("John Doe");
        var externalUserId = "zitadel-user-123";

        _userLookupService.GetExternalUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(externalUserId);
        _zitadelClient.UpdateUserProfileAsync(externalUserId, displayName.Value, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.PushDisplayNameAsync(userId, displayName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to update display name in Zitadel");
    }

    [Fact]
    public async Task PushDisplayNameAsync_WhenExternalUserIdEmpty_ReturnsFailure()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("John Doe");

        _userLookupService.GetExternalUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        // Act
        var result = await _sut.PushDisplayNameAsync(userId, displayName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("External user ID not found");
    }

    #endregion

    #region PullDisplayNameAsync Tests

    [Fact]
    public async Task PullDisplayNameAsync_WithValidProfile_ReturnsDisplayName()
    {
        // Arrange
        var externalUserId = "zitadel-user-123";
        var zitadelProfile = new ZitadelUserProfile
        {
            UserId = externalUserId,
            DisplayName = "John Doe",
            FirstName = "John",
            LastName = "Doe"
        };

        _zitadelClient.GetUserProfileAsync(externalUserId, Arg.Any<CancellationToken>())
            .Returns(zitadelProfile);

        // Act
        var result = await _sut.PullDisplayNameAsync(externalUserId);

        // Assert
        result.Should().NotBeNull();
        result!.Should().Be(DisplayName.From("John Doe"));
    }

    [Fact]
    public async Task PullDisplayNameAsync_WhenProfileNotFound_ReturnsNull()
    {
        // Arrange
        var externalUserId = "zitadel-user-123";

        _zitadelClient.GetUserProfileAsync(externalUserId, Arg.Any<CancellationToken>())
            .Returns((ZitadelUserProfile?)null);

        // Act
        var result = await _sut.PullDisplayNameAsync(externalUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task PullDisplayNameAsync_WithInvalidDisplayName_ReturnsNull()
    {
        // Arrange
        var externalUserId = "zitadel-user-123";
        var zitadelProfile = new ZitadelUserProfile
        {
            UserId = externalUserId,
            DisplayName = "", // Invalid - empty
            FirstName = "John",
            LastName = "Doe"
        };

        _zitadelClient.GetUserProfileAsync(externalUserId, Arg.Any<CancellationToken>())
            .Returns(zitadelProfile);

        // Act
        var result = await _sut.PullDisplayNameAsync(externalUserId);

        // Assert
        result.Should().BeNull(); // Validation should fail
    }

    #endregion

    #region SyncFromZitadelAsync Tests

    [Fact]
    public async Task SyncFromZitadelAsync_OnFirstSync_UpdatesFromZitadel()
    {
        // Arrange
        var profile = CreateTestProfile(lastSyncedAt: null); // First sync
        var externalUserId = "zitadel-user-123";
        var zitadelDisplayName = "Updated Name";
        var zitadelProfile = new ZitadelUserProfile
        {
            UserId = externalUserId,
            DisplayName = zitadelDisplayName,
            FirstName = "Updated",
            LastName = "Name"
        };

        _zitadelClient.GetUserProfileAsync(externalUserId, Arg.Any<CancellationToken>())
            .Returns(zitadelProfile);

        // Act
        var result = await _sut.SyncFromZitadelAsync(profile, externalUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WasUpdated.Should().BeTrue();
        result.Value.DisplayName.Value.Should().Be(zitadelDisplayName);
    }

    [Fact]
    public async Task SyncFromZitadelAsync_OnFirstSyncWithSameDisplayName_ReturnsNoUpdate()
    {
        // Arrange
        var displayName = "John Doe";
        var profile = CreateTestProfile(displayName: displayName, lastSyncedAt: null); // First sync
        var externalUserId = "zitadel-user-123";
        var zitadelProfile = new ZitadelUserProfile
        {
            UserId = externalUserId,
            DisplayName = displayName, // Same as local
            FirstName = "John",
            LastName = "Doe"
        };

        _zitadelClient.GetUserProfileAsync(externalUserId, Arg.Any<CancellationToken>())
            .Returns(zitadelProfile);

        // Act
        var result = await _sut.SyncFromZitadelAsync(profile, externalUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WasUpdated.Should().BeFalse();
    }

    [Fact]
    public async Task SyncFromZitadelAsync_WhenAlreadySynced_ReturnsNoUpdate()
    {
        // Arrange
        var profile = CreateTestProfile(lastSyncedAt: DateTime.UtcNow.AddHours(-1)); // Already synced
        var externalUserId = "zitadel-user-123";
        var zitadelProfile = new ZitadelUserProfile
        {
            UserId = externalUserId,
            DisplayName = "Different Name",
            FirstName = "Different",
            LastName = "Name"
        };

        _zitadelClient.GetUserProfileAsync(externalUserId, Arg.Any<CancellationToken>())
            .Returns(zitadelProfile);

        // Act
        var result = await _sut.SyncFromZitadelAsync(profile, externalUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WasUpdated.Should().BeFalse(); // Already synced, prefer local
        result.Value.DisplayName.Should().Be(profile.DisplayName);
    }

    [Fact]
    public async Task SyncFromZitadelAsync_WhenZitadelProfileNotFound_ReturnsNoUpdate()
    {
        // Arrange
        var profile = CreateTestProfile(lastSyncedAt: null);
        var externalUserId = "zitadel-user-123";

        _zitadelClient.GetUserProfileAsync(externalUserId, Arg.Any<CancellationToken>())
            .Returns((ZitadelUserProfile?)null);

        // Act
        var result = await _sut.SyncFromZitadelAsync(profile, externalUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WasUpdated.Should().BeFalse();
        result.Value.DisplayName.Should().Be(profile.DisplayName);
    }

    [Fact]
    public async Task SyncFromZitadelAsync_WithInvalidZitadelDisplayName_ContinuesWithLocalData()
    {
        // Arrange
        var profile = CreateTestProfile(displayName: "Valid Name", lastSyncedAt: null);
        var externalUserId = "zitadel-user-123";
        var zitadelProfile = new ZitadelUserProfile
        {
            UserId = externalUserId,
            DisplayName = "", // Invalid - will fail validation
            FirstName = "",
            LastName = ""
        };

        _zitadelClient.GetUserProfileAsync(externalUserId, Arg.Any<CancellationToken>())
            .Returns(zitadelProfile);

        // Act
        var result = await _sut.SyncFromZitadelAsync(profile, externalUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WasUpdated.Should().BeFalse();
        result.Value.DisplayName.Value.Should().Be("Valid Name");
    }

    #endregion

    #region Helper Methods

    private static Modules.UserProfile.Domain.Aggregates.UserProfile CreateTestProfile(
        string displayName = "John Doe",
        DateTime? lastSyncedAt = null)
    {
        var userId = UserId.New();
        var profile = Modules.UserProfile.Domain.Aggregates.UserProfile.Create(
            userId,
            DisplayName.From(displayName));

        if (lastSyncedAt.HasValue)
        {
            // Mark as synced to set LastSyncedAt
            profile.MarkSynced();
        }

        return profile;
    }

    #endregion
}
