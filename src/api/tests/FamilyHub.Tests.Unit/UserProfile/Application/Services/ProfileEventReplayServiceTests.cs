using FamilyHub.Modules.UserProfile.Application.Services.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.Events.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Tests.Unit.UserProfile.Application.Services;

/// <summary>
/// Unit tests for the ProfileEventReplayService.
/// </summary>
public class ProfileEventReplayServiceTests
{
    private readonly IProfileEventStore _eventStore;
    private readonly ProfileEventReplayService _sut;

    public ProfileEventReplayServiceTests()
    {
        _eventStore = Substitute.For<IProfileEventStore>();
        _sut = new ProfileEventReplayService(_eventStore);
    }

    #region ReplayEventsAsync Tests

    [Fact]
    public async Task ReplayEventsAsync_WithCreatedEvent_ReconstructsInitialState()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var displayName = "John Doe";
        var occurredAt = DateTime.UtcNow;

        var events = new List<ProfileEvent>
        {
            new ProfileCreatedEvent(
                Guid.NewGuid(),
                profileId,
                userId,
                occurredAt,
                1,
                userId,
                displayName)
        };

        _eventStore.GetLatestSnapshotAsync(profileId, Arg.Any<CancellationToken>())
            .Returns((ProfileSnapshotEvent?)null);
        _eventStore.GetEventsFromVersionAsync(profileId, 0, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var result = await _sut.ReplayEventsAsync(profileId);

        // Assert
        result.Should().NotBeNull();
        result.ProfileId.Should().Be(profileId);
        result.UserId.Should().Be(userId);
        result.DisplayName.Should().Be(displayName);
        result.Version.Should().Be(1);
    }

    [Fact]
    public async Task ReplayEventsAsync_WithCreatedAndUpdatedEvents_AppliesEventsInOrder()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var initialName = "John Doe";
        var updatedName = "Jane Doe";
        var now = DateTime.UtcNow;

        var events = new List<ProfileEvent>
        {
            new ProfileCreatedEvent(
                Guid.NewGuid(), profileId, userId, now, 1, userId, initialName),
            new ProfileFieldUpdatedEvent(
                Guid.NewGuid(), profileId, userId, now.AddMinutes(1), 2,
                nameof(ProfileStateDto.DisplayName), initialName, updatedName)
        };

        _eventStore.GetLatestSnapshotAsync(profileId, Arg.Any<CancellationToken>())
            .Returns((ProfileSnapshotEvent?)null);
        _eventStore.GetEventsFromVersionAsync(profileId, 0, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var result = await _sut.ReplayEventsAsync(profileId);

        // Assert
        result.DisplayName.Should().Be(updatedName);
        result.Version.Should().Be(2);
    }

    [Fact]
    public async Task ReplayEventsAsync_WithBirthdayUpdate_SetsCorrectValue()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var birthday = "1990-05-15";
        var now = DateTime.UtcNow;

        var events = new List<ProfileEvent>
        {
            new ProfileCreatedEvent(
                Guid.NewGuid(), profileId, userId, now, 1, userId, "Test User"),
            new ProfileFieldUpdatedEvent(
                Guid.NewGuid(), profileId, userId, now.AddMinutes(1), 2,
                nameof(ProfileStateDto.Birthday), null, birthday)
        };

        _eventStore.GetLatestSnapshotAsync(profileId, Arg.Any<CancellationToken>())
            .Returns((ProfileSnapshotEvent?)null);
        _eventStore.GetEventsFromVersionAsync(profileId, 0, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var result = await _sut.ReplayEventsAsync(profileId);

        // Assert
        result.Birthday.Should().Be(DateOnly.Parse(birthday));
    }

    [Fact]
    public async Task ReplayEventsAsync_WithSnapshot_StartsFromSnapshotVersion()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var snapshotVersion = 50;
        var now = DateTime.UtcNow;

        var snapshotState = new ProfileStateDto
        {
            ProfileId = profileId,
            UserId = userId,
            DisplayName = "Snapshot User",
            Version = snapshotVersion
        };
        var snapshotJson = System.Text.Json.JsonSerializer.Serialize(snapshotState);

        var snapshot = new ProfileSnapshotEvent(
            Guid.NewGuid(), profileId, userId, now, snapshotVersion, snapshotJson);

        var eventsAfterSnapshot = new List<ProfileEvent>
        {
            new ProfileFieldUpdatedEvent(
                Guid.NewGuid(), profileId, userId, now.AddMinutes(1), 51,
                nameof(ProfileStateDto.DisplayName), "Snapshot User", "Updated User")
        };

        _eventStore.GetLatestSnapshotAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(snapshot);
        _eventStore.GetEventsFromVersionAsync(profileId, snapshotVersion, Arg.Any<CancellationToken>())
            .Returns(eventsAfterSnapshot);

        // Act
        var result = await _sut.ReplayEventsAsync(profileId);

        // Assert
        result.DisplayName.Should().Be("Updated User");
        result.Version.Should().Be(51);
    }

    #endregion

    #region CreateSnapshotIfNeededAsync Tests

    [Fact]
    public async Task CreateSnapshotIfNeededAsync_WhenBelowThreshold_DoesNotCreateSnapshot()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();

        _eventStore.GetCurrentVersionAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(30); // Below 50 threshold
        _eventStore.GetLatestSnapshotAsync(profileId, Arg.Any<CancellationToken>())
            .Returns((ProfileSnapshotEvent?)null);

        // Act
        var result = await _sut.CreateSnapshotIfNeededAsync(profileId, userId);

        // Assert
        result.Should().BeFalse();
        await _eventStore.DidNotReceive().AppendEventAsync(
            Arg.Any<ProfileSnapshotEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSnapshotIfNeededAsync_WhenAtThreshold_CreatesSnapshot()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var now = DateTime.UtcNow;

        _eventStore.GetCurrentVersionAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(50); // At threshold
        _eventStore.GetLatestSnapshotAsync(profileId, Arg.Any<CancellationToken>())
            .Returns((ProfileSnapshotEvent?)null);
        _eventStore.GetEventsFromVersionAsync(profileId, 0, Arg.Any<CancellationToken>())
            .Returns(new List<ProfileEvent>
            {
                new ProfileCreatedEvent(Guid.NewGuid(), profileId, userId, now, 1, userId, "Test")
            });

        // Act
        var result = await _sut.CreateSnapshotIfNeededAsync(profileId, userId);

        // Assert
        result.Should().BeTrue();
        await _eventStore.Received(1).AppendEventAsync(
            Arg.Is<ProfileSnapshotEvent>(e => e.Version == 51), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSnapshotIfNeededAsync_WhenRecentSnapshotExists_DoesNotCreateSnapshot()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var now = DateTime.UtcNow;

        _eventStore.GetCurrentVersionAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(70); // 70 total events
        _eventStore.GetLatestSnapshotAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSnapshotEvent(
                Guid.NewGuid(), profileId, userId, now, 50, "{}")); // Snapshot at 50

        // 70 - 50 = 20 events since snapshot (below threshold)

        // Act
        var result = await _sut.CreateSnapshotIfNeededAsync(profileId, userId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ReplayEventsAtTimeAsync Tests

    [Fact]
    public async Task ReplayEventsAtTimeAsync_BeforeAnyEvents_ReturnsNull()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var asOf = DateTime.UtcNow.AddDays(-1);

        _eventStore.GetEventsAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(new List<ProfileEvent>());

        // Act
        var result = await _sut.ReplayEventsAtTimeAsync(profileId, asOf);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ReplayEventsAtTimeAsync_AtSpecificTime_ReturnsStateAtThatTime()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var createdAt = DateTime.UtcNow.AddHours(-2);
        var updatedAt = DateTime.UtcNow.AddHours(-1);
        var asOf = DateTime.UtcNow.AddMinutes(-90); // Between created and updated

        var events = new List<ProfileEvent>
        {
            new ProfileCreatedEvent(
                Guid.NewGuid(), profileId, userId, createdAt, 1, userId, "Initial Name"),
            new ProfileFieldUpdatedEvent(
                Guid.NewGuid(), profileId, userId, updatedAt, 2,
                nameof(ProfileStateDto.DisplayName), "Initial Name", "Updated Name")
        };

        _eventStore.GetEventsAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var result = await _sut.ReplayEventsAtTimeAsync(profileId, asOf);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("Initial Name"); // Only created event before asOf
        result.Version.Should().Be(1);
    }

    #endregion
}
