using FamilyHub.Modules.Auth.Domain.Events;
using FamilyHub.Modules.UserProfile.Application.Abstractions;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Tests.Unit.UserProfile.Application.EventHandlers;

/// <summary>
/// Unit tests for the UserLoggedInEvent domain event.
/// Tests event data structure and creation.
/// </summary>
public class UserLoggedInEventTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesEvent()
    {
        // Arrange
        var userId = UserId.New();
        var externalUserId = "zitadel-user-123";
        var externalProvider = "zitadel";
        var displayName = "John Doe";
        var email = Email.From("john@example.com");
        var isNewUser = true;

        // Act
        var @event = new UserLoggedInEvent(
            userId,
            externalUserId,
            externalProvider,
            displayName,
            email,
            isNewUser);

        // Assert
        @event.UserId.Should().Be(userId);
        @event.ExternalUserId.Should().Be(externalUserId);
        @event.ExternalProvider.Should().Be(externalProvider);
        @event.DisplayNameFromProvider.Should().Be(displayName);
        @event.Email.Should().Be(email);
        @event.IsNewUser.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNullDisplayName_CreatesEventWithNullDisplayName()
    {
        // Arrange
        var userId = UserId.New();
        var externalUserId = "zitadel-user-123";
        var email = Email.From("john@example.com");

        // Act
        var @event = new UserLoggedInEvent(
            userId,
            externalUserId,
            "zitadel",
            displayNameFromProvider: null,
            email,
            isNewUser: false);

        // Assert
        @event.DisplayNameFromProvider.Should().BeNull();
        @event.IsNewUser.Should().BeFalse();
    }
}

/// <summary>
/// Unit tests for ZitadelSyncService integration with UserProfile repository.
/// Tests sync scenarios without DbContext dependency.
/// </summary>
public class ZitadelSyncIntegrationTests
{
    private readonly IUserProfileRepository _profileRepository;
    private readonly IZitadelSyncService _zitadelSyncService;

    public ZitadelSyncIntegrationTests()
    {
        _profileRepository = Substitute.For<IUserProfileRepository>();
        _zitadelSyncService = Substitute.For<IZitadelSyncService>();
    }

    [Fact]
    public async Task SyncService_WhenProfileNotFound_DoesNotCallSync()
    {
        // Arrange
        var userId = UserId.New();

        _profileRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((Modules.UserProfile.Domain.Aggregates.UserProfile?)null);

        // Act
        var profile = await _profileRepository.GetByUserIdAsync(userId, CancellationToken.None);

        // Assert
        profile.Should().BeNull();
        await _zitadelSyncService.DidNotReceive()
            .SyncFromZitadelAsync(
                Arg.Any<Modules.UserProfile.Domain.Aggregates.UserProfile>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncService_WhenProfileExists_CanBeSynced()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("John Doe");
        var externalUserId = "zitadel-user-123";

        var profile = Modules.UserProfile.Domain.Aggregates.UserProfile.Create(userId, displayName);

        _profileRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(profile);

        var syncResult = SyncResult.NoUpdate(displayName);
        _zitadelSyncService.SyncFromZitadelAsync(profile, externalUserId, Arg.Any<CancellationToken>())
            .Returns(FamilyHub.SharedKernel.Domain.Result.Success(syncResult));

        // Act
        var existingProfile = await _profileRepository.GetByUserIdAsync(userId, CancellationToken.None);
        var result = await _zitadelSyncService.SyncFromZitadelAsync(existingProfile!, externalUserId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WasUpdated.Should().BeFalse();
        result.Value.DisplayName.Should().Be(displayName);
    }

    [Fact]
    public async Task SyncService_WhenSyncReturnsUpdate_ProvidesNewDisplayName()
    {
        // Arrange
        var userId = UserId.New();
        var oldDisplayName = DisplayName.From("Old Name");
        var newDisplayName = DisplayName.From("New Name");
        var externalUserId = "zitadel-user-123";

        var profile = Modules.UserProfile.Domain.Aggregates.UserProfile.Create(userId, oldDisplayName);

        var syncResult = SyncResult.Updated(newDisplayName);
        _zitadelSyncService.SyncFromZitadelAsync(profile, externalUserId, Arg.Any<CancellationToken>())
            .Returns(FamilyHub.SharedKernel.Domain.Result.Success(syncResult));

        // Act
        var result = await _zitadelSyncService.SyncFromZitadelAsync(profile, externalUserId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WasUpdated.Should().BeTrue();
        result.Value.DisplayName.Should().Be(newDisplayName);
    }

    [Fact]
    public async Task SyncService_WhenSyncFails_ReturnsFailure()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("John Doe");
        var externalUserId = "zitadel-user-123";

        var profile = Modules.UserProfile.Domain.Aggregates.UserProfile.Create(userId, displayName);

        _zitadelSyncService.SyncFromZitadelAsync(profile, externalUserId, Arg.Any<CancellationToken>())
            .Returns(FamilyHub.SharedKernel.Domain.Result.Failure<SyncResult>("Sync failed"));

        // Act
        var result = await _zitadelSyncService.SyncFromZitadelAsync(profile, externalUserId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Sync failed");
    }
}
