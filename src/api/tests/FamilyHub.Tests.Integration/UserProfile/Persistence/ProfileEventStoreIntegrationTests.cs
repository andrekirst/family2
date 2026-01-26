using FamilyHub.Modules.UserProfile.Domain.Events.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.Modules.UserProfile.Persistence;
using FamilyHub.Modules.UserProfile.Persistence.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Tests.Integration.UserProfile.Persistence;

/// <summary>
/// Integration tests for ProfileEventStore using real PostgreSQL.
/// Tests event persistence, retrieval, and JSONB handling.
/// </summary>
[Collection("UserProfileDatabase")]
public sealed class ProfileEventStoreIntegrationTests(UserProfilePostgreSqlContainerFixture fixture)
{
    /// <summary>
    /// Creates a fresh DbContext for each test to avoid change tracker state issues.
    /// </summary>
    private UserProfileDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UserProfileDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new UserProfileDbContext(options);
    }

    /// <summary>
    /// Creates the event store with a fresh context.
    /// </summary>
    private (IProfileEventStore EventStore, UserProfileDbContext Context) CreateEventStore()
    {
        var context = CreateContext();
        var eventStore = new ProfileEventStore(context);
        return (eventStore, context);
    }

    #region AppendEventAsync Tests

    [Fact]
    public async Task AppendEventAsync_WithCreatedEvent_PersistsToDatabase()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = await CreateTestProfileViaSqlAsync(context);
            var userId = UserId.New();
            var @event = new ProfileCreatedEvent(
                Guid.NewGuid(),
                profileId,
                userId,
                DateTime.UtcNow,
                1,
                userId,
                "Test User");

            // Act
            await eventStore.AppendEventAsync(@event);
            await context.SaveChangesAsync();

            // Assert
            var entity = await context.ProfileEvents
                .FirstOrDefaultAsync(e => e.ProfileId == profileId.Value);
            entity.Should().NotBeNull();
            entity!.EventType.Should().Be(nameof(ProfileCreatedEvent));
            entity.Version.Should().Be(1);
        }
    }

    [Fact]
    public async Task AppendEventAsync_WithFieldUpdatedEvent_StoresJsonCorrectly()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = await CreateTestProfileViaSqlAsync(context);
            var userId = UserId.New();
            var oldValue = "Old Name";
            var newValue = "New Name";

            var @event = new ProfileFieldUpdatedEvent(
                Guid.NewGuid(),
                profileId,
                userId,
                DateTime.UtcNow,
                1,
                "DisplayName",
                oldValue,
                newValue);

            // Act
            await eventStore.AppendEventAsync(@event);
            await context.SaveChangesAsync();

            // Assert
            var entity = await context.ProfileEvents
                .FirstOrDefaultAsync(e => e.ProfileId == profileId.Value);
            entity.Should().NotBeNull();
            entity!.EventData.Should().Contain("oldValue");
            entity.EventData.Should().Contain("newValue");
            entity.EventData.Should().Contain(oldValue);
            entity.EventData.Should().Contain(newValue);
        }
    }

    #endregion

    #region GetEventsAsync Tests

    [Fact]
    public async Task GetEventsAsync_WithMultipleEvents_ReturnsInVersionOrder()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = await CreateTestProfileViaSqlAsync(context);
            var userId = UserId.New();
            var now = DateTime.UtcNow;

            var events = new List<ProfileEvent>
            {
                new ProfileCreatedEvent(Guid.NewGuid(), profileId, userId, now, 1, userId, "User 1"),
                new ProfileFieldUpdatedEvent(Guid.NewGuid(), profileId, userId, now.AddMinutes(1), 2, "DisplayName", "User 1", "User 2"),
                new ProfileFieldUpdatedEvent(Guid.NewGuid(), profileId, userId, now.AddMinutes(2), 3, "DisplayName", "User 2", "User 3")
            };

            foreach (var @event in events)
            {
                await eventStore.AppendEventAsync(@event);
            }
            await context.SaveChangesAsync();

            // Act
            var result = await eventStore.GetEventsAsync(profileId);

            // Assert
            result.Should().HaveCount(3);
            result[0].Version.Should().Be(1);
            result[1].Version.Should().Be(2);
            result[2].Version.Should().Be(3);
        }
    }

    [Fact]
    public async Task GetEventsAsync_WithNoEvents_ReturnsEmptyList()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = UserProfileId.New();

            // Act
            var result = await eventStore.GetEventsAsync(profileId);

            // Assert
            result.Should().BeEmpty();
        }
    }

    #endregion

    #region GetEventsFromVersionAsync Tests

    [Fact]
    public async Task GetEventsFromVersionAsync_ReturnsEventsAfterVersion()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = await CreateTestProfileViaSqlAsync(context);
            var userId = UserId.New();
            var now = DateTime.UtcNow;

            await eventStore.AppendEventAsync(new ProfileCreatedEvent(Guid.NewGuid(), profileId, userId, now, 1, userId, "User"));
            await eventStore.AppendEventAsync(new ProfileFieldUpdatedEvent(Guid.NewGuid(), profileId, userId, now.AddMinutes(1), 2, "DisplayName", "User", "User 2"));
            await eventStore.AppendEventAsync(new ProfileFieldUpdatedEvent(Guid.NewGuid(), profileId, userId, now.AddMinutes(2), 3, "DisplayName", "User 2", "User 3"));
            await context.SaveChangesAsync();

            // Act
            var result = await eventStore.GetEventsFromVersionAsync(profileId, 1);

            // Assert
            result.Should().HaveCount(2);
            result[0].Version.Should().Be(2);
            result[1].Version.Should().Be(3);
        }
    }

    #endregion

    #region GetLatestSnapshotAsync Tests

    [Fact]
    public async Task GetLatestSnapshotAsync_WithNoSnapshots_ReturnsNull()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = await CreateTestProfileViaSqlAsync(context);
            var userId = UserId.New();

            await eventStore.AppendEventAsync(new ProfileCreatedEvent(
                Guid.NewGuid(), profileId, userId, DateTime.UtcNow, 1, userId, "User"));
            await context.SaveChangesAsync();

            // Act
            var result = await eventStore.GetLatestSnapshotAsync(profileId);

            // Assert
            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetLatestSnapshotAsync_WithMultipleSnapshots_ReturnsLatest()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = await CreateTestProfileViaSqlAsync(context);
            var userId = UserId.New();
            var now = DateTime.UtcNow;

            await eventStore.AppendEventAsync(new ProfileSnapshotEvent(
                Guid.NewGuid(), profileId, userId, now, 10, "{\"version\":10}"));
            await eventStore.AppendEventAsync(new ProfileSnapshotEvent(
                Guid.NewGuid(), profileId, userId, now.AddMinutes(1), 20, "{\"version\":20}"));
            await context.SaveChangesAsync();

            // Act
            var result = await eventStore.GetLatestSnapshotAsync(profileId);

            // Assert
            result.Should().NotBeNull();
            result!.Version.Should().Be(20);
            result.SnapshotJson.Should().Contain("20");
        }
    }

    #endregion

    #region GetCurrentVersionAsync Tests

    [Fact]
    public async Task GetCurrentVersionAsync_WithEvents_ReturnsHighestVersion()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = await CreateTestProfileViaSqlAsync(context);
            var userId = UserId.New();
            var now = DateTime.UtcNow;

            await eventStore.AppendEventAsync(new ProfileCreatedEvent(Guid.NewGuid(), profileId, userId, now, 1, userId, "User"));
            await eventStore.AppendEventAsync(new ProfileFieldUpdatedEvent(Guid.NewGuid(), profileId, userId, now.AddMinutes(1), 2, "DisplayName", "User", "User 2"));
            await eventStore.AppendEventAsync(new ProfileFieldUpdatedEvent(Guid.NewGuid(), profileId, userId, now.AddMinutes(2), 5, "DisplayName", "User 2", "User 3"));
            await context.SaveChangesAsync();

            // Act
            var result = await eventStore.GetCurrentVersionAsync(profileId);

            // Assert
            result.Should().Be(5);
        }
    }

    [Fact]
    public async Task GetCurrentVersionAsync_WithNoEvents_ReturnsZero()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = UserProfileId.New();

            // Act
            var result = await eventStore.GetCurrentVersionAsync(profileId);

            // Assert
            result.Should().Be(0);
        }
    }

    #endregion

    #region HasEventsAsync Tests

    [Fact]
    public async Task HasEventsAsync_WithEvents_ReturnsTrue()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = await CreateTestProfileViaSqlAsync(context);
            var userId = UserId.New();

            await eventStore.AppendEventAsync(new ProfileCreatedEvent(
                Guid.NewGuid(), profileId, userId, DateTime.UtcNow, 1, userId, "User"));
            await context.SaveChangesAsync();

            // Act
            var result = await eventStore.HasEventsAsync(profileId);

            // Assert
            result.Should().BeTrue();
        }
    }

    [Fact]
    public async Task HasEventsAsync_WithNoEvents_ReturnsFalse()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = UserProfileId.New();

            // Act
            var result = await eventStore.HasEventsAsync(profileId);

            // Assert
            result.Should().BeFalse();
        }
    }

    #endregion

    #region Event Deserialization Tests

    [Fact]
    public async Task GetEventsAsync_DeserializesAllEventTypesCorrectly()
    {
        // Arrange
        var (eventStore, context) = CreateEventStore();
        await using (context)
        {
            var profileId = await CreateTestProfileViaSqlAsync(context);
            var userId = UserId.New();
            var now = DateTime.UtcNow;

            await eventStore.AppendEventAsync(new ProfileCreatedEvent(
                Guid.NewGuid(), profileId, userId, now, 1, userId, "Test User"));
            await eventStore.AppendEventAsync(new ProfileFieldUpdatedEvent(
                Guid.NewGuid(), profileId, userId, now.AddMinutes(1), 2, "Birthday", null, "1990-05-15"));
            await eventStore.AppendEventAsync(new ProfileSnapshotEvent(
                Guid.NewGuid(), profileId, userId, now.AddMinutes(2), 3, "{\"displayName\":\"Test\"}"));
            await context.SaveChangesAsync();

            // Act
            var events = await eventStore.GetEventsAsync(profileId);

            // Assert
            events.Should().HaveCount(3);
            events[0].Should().BeOfType<ProfileCreatedEvent>();
            events[1].Should().BeOfType<ProfileFieldUpdatedEvent>();
            events[2].Should().BeOfType<ProfileSnapshotEvent>();

            var created = (ProfileCreatedEvent)events[0];
            created.UserId.Should().Be(userId);
            created.DisplayName.Should().Be("Test User");

            var updated = (ProfileFieldUpdatedEvent)events[1];
            updated.FieldName.Should().Be("Birthday");
            updated.OldValue.Should().BeNull();
            updated.NewValue.Should().Be("1990-05-15");

            var snapshot = (ProfileSnapshotEvent)events[2];
            snapshot.SnapshotJson.Should().Contain("Test");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test profile in the database via raw SQL and returns its ID.
    /// Uses raw SQL to avoid EF Core's identity map issues with Vogen value objects.
    /// Required because events have a foreign key to the profiles table.
    /// </summary>
    private static async Task<UserProfileId> CreateTestProfileViaSqlAsync(UserProfileDbContext context)
    {
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var displayName = $"Test User {Guid.NewGuid():N}";
        var now = DateTime.UtcNow;

        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO user_profile.profiles (
                id, user_id, display_name,
                language, timezone, date_format,
                birthday_visibility, pronouns_visibility, preferences_visibility,
                created_at, updated_at
            ) VALUES (
                {0}, {1}, {2},
                'en', 'UTC', 'yyyy-MM-dd',
                'Family', 'Family', 'Hidden',
                {3}, {3}
            )",
            profileId, userId, displayName, now);

        return UserProfileId.From(profileId);
    }

    #endregion
}
