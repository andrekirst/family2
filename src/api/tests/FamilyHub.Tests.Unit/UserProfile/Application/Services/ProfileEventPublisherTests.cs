using FamilyHub.Modules.UserProfile.Application.Services;
using FamilyHub.Modules.UserProfile.Domain.Events;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.Tests.Unit.UserProfile.Application.Services;

/// <summary>
/// Unit tests for ProfileEventPublisher.
/// Tests RabbitMQ event publishing with correct exchange and routing keys.
/// </summary>
public class ProfileEventPublisherTests
{
    private const string ExpectedExchange = "profile.events";

    private readonly IMessageBrokerPublisher _messageBrokerPublisher;
    private readonly ILogger<ProfileEventPublisher> _logger;
    private readonly ProfileEventPublisher _publisher;

    public ProfileEventPublisherTests()
    {
        _messageBrokerPublisher = Substitute.For<IMessageBrokerPublisher>();
        _logger = Substitute.For<ILogger<ProfileEventPublisher>>();

        _publisher = new ProfileEventPublisher(_messageBrokerPublisher, _logger);
    }

    #region BirthdaySetEvent Publishing Tests

    [Fact]
    public async Task PublishBirthdaySetAsync_PublishesToCorrectExchange()
    {
        // Arrange
        var @event = CreateBirthdaySetEvent();

        // Act
        await _publisher.PublishBirthdaySetAsync(@event, CancellationToken.None);

        // Assert
        await _messageBrokerPublisher.Received(1).PublishAsync(
            ExpectedExchange,
            Arg.Any<string>(),
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishBirthdaySetAsync_PublishesWithCorrectRoutingKey()
    {
        // Arrange
        var @event = CreateBirthdaySetEvent();
        const string expectedRoutingKey = "profile.birthday.set";

        // Act
        await _publisher.PublishBirthdaySetAsync(@event, CancellationToken.None);

        // Assert
        await _messageBrokerPublisher.Received(1).PublishAsync(
            Arg.Any<string>(),
            expectedRoutingKey,
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishBirthdaySetAsync_PublishesIntegrationEvent()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var birthday = Birthday.From(new DateOnly(1990, 5, 15));
        var displayName = DisplayName.From("John Doe");

        var @event = new BirthdaySetEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            birthday: birthday,
            displayName: displayName);

        object? capturedPayload = null;
        await _messageBrokerPublisher.PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<object>(p => capturedPayload = p),
            Arg.Any<CancellationToken>());

        // Act
        await _publisher.PublishBirthdaySetAsync(@event, CancellationToken.None);

        // Assert
        capturedPayload.Should().NotBeNull();

        // Verify payload structure using reflection (since it's internal)
        var payloadType = capturedPayload!.GetType();
        payloadType.GetProperty("ProfileId")?.GetValue(capturedPayload).Should().Be(profileId.Value);
        payloadType.GetProperty("UserId")?.GetValue(capturedPayload).Should().Be(userId.Value);
        payloadType.GetProperty("Birthday")?.GetValue(capturedPayload).Should().Be(birthday.Value);
        payloadType.GetProperty("DisplayName")?.GetValue(capturedPayload).Should().Be(displayName.Value);
    }

    #endregion

    #region DisplayNameChangedEvent Publishing Tests

    [Fact]
    public async Task PublishDisplayNameChangedAsync_PublishesToCorrectExchange()
    {
        // Arrange
        var @event = CreateDisplayNameChangedEvent();

        // Act
        await _publisher.PublishDisplayNameChangedAsync(@event, CancellationToken.None);

        // Assert
        await _messageBrokerPublisher.Received(1).PublishAsync(
            ExpectedExchange,
            Arg.Any<string>(),
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishDisplayNameChangedAsync_PublishesWithCorrectRoutingKey()
    {
        // Arrange
        var @event = CreateDisplayNameChangedEvent();
        const string expectedRoutingKey = "profile.name.changed";

        // Act
        await _publisher.PublishDisplayNameChangedAsync(@event, CancellationToken.None);

        // Assert
        await _messageBrokerPublisher.Received(1).PublishAsync(
            Arg.Any<string>(),
            expectedRoutingKey,
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishDisplayNameChangedAsync_PublishesIntegrationEvent()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var oldDisplayName = DisplayName.From("John Doe");
        var newDisplayName = DisplayName.From("John Smith");

        var @event = new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldDisplayName: oldDisplayName,
            newDisplayName: newDisplayName);

        object? capturedPayload = null;
        await _messageBrokerPublisher.PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<object>(p => capturedPayload = p),
            Arg.Any<CancellationToken>());

        // Act
        await _publisher.PublishDisplayNameChangedAsync(@event, CancellationToken.None);

        // Assert
        capturedPayload.Should().NotBeNull();

        var payloadType = capturedPayload!.GetType();
        payloadType.GetProperty("ProfileId")?.GetValue(capturedPayload).Should().Be(profileId.Value);
        payloadType.GetProperty("UserId")?.GetValue(capturedPayload).Should().Be(userId.Value);
        payloadType.GetProperty("OldDisplayName")?.GetValue(capturedPayload).Should().Be(oldDisplayName.Value);
        payloadType.GetProperty("NewDisplayName")?.GetValue(capturedPayload).Should().Be(newDisplayName.Value);
    }

    #endregion

    #region PreferencesUpdatedEvent Publishing Tests

    [Fact]
    public async Task PublishPreferencesUpdatedAsync_PublishesToCorrectExchange()
    {
        // Arrange
        var @event = CreatePreferencesUpdatedEvent();

        // Act
        await _publisher.PublishPreferencesUpdatedAsync(@event, CancellationToken.None);

        // Assert
        await _messageBrokerPublisher.Received(1).PublishAsync(
            ExpectedExchange,
            Arg.Any<string>(),
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishPreferencesUpdatedAsync_PublishesWithCorrectRoutingKey()
    {
        // Arrange
        var @event = CreatePreferencesUpdatedEvent();
        const string expectedRoutingKey = "profile.preferences.updated";

        // Act
        await _publisher.PublishPreferencesUpdatedAsync(@event, CancellationToken.None);

        // Assert
        await _messageBrokerPublisher.Received(1).PublishAsync(
            Arg.Any<string>(),
            expectedRoutingKey,
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishPreferencesUpdatedAsync_PublishesIntegrationEvent()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var oldLanguage = "en";
        var newLanguage = "de";
        var oldTimezone = "UTC";
        var newTimezone = "Europe/Berlin";

        var @event = new PreferencesUpdatedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldLanguage: oldLanguage,
            newLanguage: newLanguage,
            oldTimezone: oldTimezone,
            newTimezone: newTimezone);

        object? capturedPayload = null;
        await _messageBrokerPublisher.PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<object>(p => capturedPayload = p),
            Arg.Any<CancellationToken>());

        // Act
        await _publisher.PublishPreferencesUpdatedAsync(@event, CancellationToken.None);

        // Assert
        capturedPayload.Should().NotBeNull();

        var payloadType = capturedPayload!.GetType();
        payloadType.GetProperty("ProfileId")?.GetValue(capturedPayload).Should().Be(profileId.Value);
        payloadType.GetProperty("UserId")?.GetValue(capturedPayload).Should().Be(userId.Value);
        payloadType.GetProperty("OldLanguage")?.GetValue(capturedPayload).Should().Be(oldLanguage);
        payloadType.GetProperty("NewLanguage")?.GetValue(capturedPayload).Should().Be(newLanguage);
        payloadType.GetProperty("OldTimezone")?.GetValue(capturedPayload).Should().Be(oldTimezone);
        payloadType.GetProperty("NewTimezone")?.GetValue(capturedPayload).Should().Be(newTimezone);
    }

    [Fact]
    public async Task PublishPreferencesUpdatedAsync_WithNullValues_PublishesCorrectly()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();

        var @event = new PreferencesUpdatedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldLanguage: null,
            newLanguage: "de",
            oldTimezone: null,
            newTimezone: null);

        object? capturedPayload = null;
        await _messageBrokerPublisher.PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<object>(p => capturedPayload = p),
            Arg.Any<CancellationToken>());

        // Act
        await _publisher.PublishPreferencesUpdatedAsync(@event, CancellationToken.None);

        // Assert
        capturedPayload.Should().NotBeNull();

        var payloadType = capturedPayload!.GetType();
        payloadType.GetProperty("OldLanguage")?.GetValue(capturedPayload).Should().BeNull();
        payloadType.GetProperty("NewLanguage")?.GetValue(capturedPayload).Should().Be("de");
        payloadType.GetProperty("OldTimezone")?.GetValue(capturedPayload).Should().BeNull();
        payloadType.GetProperty("NewTimezone")?.GetValue(capturedPayload).Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private static BirthdaySetEvent CreateBirthdaySetEvent()
    {
        return new BirthdaySetEvent(
            eventVersion: 1,
            profileId: UserProfileId.New(),
            userId: UserId.New(),
            birthday: Birthday.From(new DateOnly(1990, 5, 15)),
            displayName: DisplayName.From("Test User"));
    }

    private static DisplayNameChangedEvent CreateDisplayNameChangedEvent()
    {
        return new DisplayNameChangedEvent(
            eventVersion: 1,
            profileId: UserProfileId.New(),
            userId: UserId.New(),
            oldDisplayName: DisplayName.From("Old Name"),
            newDisplayName: DisplayName.From("New Name"));
    }

    private static PreferencesUpdatedEvent CreatePreferencesUpdatedEvent()
    {
        return new PreferencesUpdatedEvent(
            eventVersion: 1,
            profileId: UserProfileId.New(),
            userId: UserId.New(),
            oldLanguage: "en",
            newLanguage: "de",
            oldTimezone: "UTC",
            newTimezone: "Europe/Berlin");
    }

    #endregion
}
