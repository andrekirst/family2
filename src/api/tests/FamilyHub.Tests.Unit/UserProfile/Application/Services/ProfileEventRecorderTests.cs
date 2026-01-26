using FamilyHub.Modules.UserProfile.Application.Services.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.Events.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using UserProfileAggregate = FamilyHub.Modules.UserProfile.Domain.Aggregates.UserProfile;

namespace FamilyHub.Tests.Unit.UserProfile.Application.Services;

/// <summary>
/// Unit tests for the ProfileEventRecorder.
/// </summary>
public class ProfileEventRecorderTests
{
    private readonly IProfileEventStore _eventStore;
    private readonly IProfileEventReplayService _replayService;
    private readonly ProfileEventRecorder _sut;

    public ProfileEventRecorderTests()
    {
        _eventStore = Substitute.For<IProfileEventStore>();
        _replayService = Substitute.For<IProfileEventReplayService>();
        _sut = new ProfileEventRecorder(_eventStore, _replayService);
    }

    #region RecordCreatedAsync Tests

    [Fact]
    public async Task RecordCreatedAsync_WithValidProfile_RecordsCreationEvent()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("Test User");
        var profile = UserProfileAggregate.Create(userId, displayName);

        // Act
        await _sut.RecordCreatedAsync(profile, userId);

        // Assert
        await _eventStore.Received(1).AppendEventAsync(
            Arg.Is<ProfileCreatedEvent>(e =>
                e.ProfileId == profile.Id &&
                e.UserId == userId &&
                e.DisplayName == displayName.Value &&
                e.Version == 1 &&
                e.ChangedBy == userId),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region RecordFieldUpdateAsync Tests

    [Fact]
    public async Task RecordFieldUpdateAsync_WithDisplayNameChange_RecordsUpdateEvent()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var oldValue = "Old Name";
        var newValue = "New Name";
        var currentVersion = 5;

        _eventStore.GetCurrentVersionAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(currentVersion);
        _replayService.CreateSnapshotIfNeededAsync(profileId, userId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.RecordFieldUpdateAsync(
            profileId, userId, nameof(ProfileStateDto.DisplayName), oldValue, newValue);

        // Assert
        await _eventStore.Received(1).AppendEventAsync(
            Arg.Is<ProfileFieldUpdatedEvent>(e =>
                e.ProfileId == profileId &&
                e.FieldName == nameof(ProfileStateDto.DisplayName) &&
                e.OldValue == oldValue &&
                e.NewValue == newValue &&
                e.Version == currentVersion + 1 &&
                e.ChangedBy == userId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordFieldUpdateAsync_AfterRecording_ChecksForSnapshot()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();

        _eventStore.GetCurrentVersionAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(49); // Just before snapshot threshold
        _replayService.CreateSnapshotIfNeededAsync(profileId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _sut.RecordFieldUpdateAsync(
            profileId, userId, nameof(ProfileStateDto.Birthday), null, "1990-01-01");

        // Assert
        await _replayService.Received(1).CreateSnapshotIfNeededAsync(
            profileId, userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordFieldUpdateAsync_WithNullValues_RecordsCorrectly()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();

        _eventStore.GetCurrentVersionAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(1);
        _replayService.CreateSnapshotIfNeededAsync(profileId, userId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act - Setting birthday from null
        await _sut.RecordFieldUpdateAsync(
            profileId, userId, nameof(ProfileStateDto.Birthday), null, "1990-05-15");

        // Assert
        await _eventStore.Received(1).AppendEventAsync(
            Arg.Is<ProfileFieldUpdatedEvent>(e =>
                e.OldValue == null &&
                e.NewValue == "1990-05-15"),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region EnsureEventsExistAsync Tests

    [Fact]
    public async Task EnsureEventsExistAsync_WhenEventsExist_DoesNotCreateEvent()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("Test User");
        var profile = UserProfileAggregate.Create(userId, displayName);

        _eventStore.HasEventsAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _sut.EnsureEventsExistAsync(profile, userId);

        // Assert
        await _eventStore.DidNotReceive().AppendEventAsync(
            Arg.Any<ProfileCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureEventsExistAsync_WhenNoEventsExist_CreatesSyntheticCreationEvent()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("Legacy User");
        var profile = UserProfileAggregate.Create(userId, displayName);

        _eventStore.HasEventsAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns(false); // No events exist (legacy profile)

        // Act
        await _sut.EnsureEventsExistAsync(profile, userId);

        // Assert - Should create a synthetic creation event
        await _eventStore.Received(1).AppendEventAsync(
            Arg.Is<ProfileCreatedEvent>(e =>
                e.ProfileId == profile.Id &&
                e.UserId == userId &&
                e.DisplayName == displayName.Value),
            Arg.Any<CancellationToken>());
    }

    #endregion
}
