using FamilyHub.Modules.UserProfile.Application.Abstractions;
using FamilyHub.Modules.UserProfile.Application.EventHandlers;
using FamilyHub.Modules.UserProfile.Domain.Events;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyHub.Tests.Unit.UserProfile.Application.EventHandlers;

/// <summary>
/// Unit tests for PreferencesUpdatedEventHandler.
/// Tests event chain: Profile → Logging (stub for future Notifications module).
/// </summary>
public class PreferencesUpdatedEventHandlerTests
{
    private readonly IProfileEventPublisher _eventPublisher;
    private readonly ILogger<PreferencesUpdatedEventHandler> _logger;
    private readonly PreferencesUpdatedEventHandler _handler;

    public PreferencesUpdatedEventHandlerTests()
    {
        _eventPublisher = Substitute.For<IProfileEventPublisher>();
        _logger = Substitute.For<ILogger<PreferencesUpdatedEventHandler>>();

        _handler = new PreferencesUpdatedEventHandler(
            _eventPublisher,
            _logger);
    }

    #region Language Change Tests

    [Fact]
    public async Task Handle_LanguageChange_PublishesToRabbitMQ()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();

        var @event = new PreferencesUpdatedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldLanguage: "en",
            newLanguage: "de",
            oldTimezone: null,
            newTimezone: null);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishPreferencesUpdatedAsync(
            @event,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LanguageChange_ShouldNotThrow()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();

        var @event = new PreferencesUpdatedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldLanguage: "en",
            newLanguage: "fr",
            oldTimezone: null,
            newTimezone: null);

        // Act
        var act = () => _handler.Handle(@event, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Timezone Change Tests

    [Fact]
    public async Task Handle_TimezoneChange_PublishesToRabbitMQ()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();

        var @event = new PreferencesUpdatedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldLanguage: null,
            newLanguage: null,
            oldTimezone: "UTC",
            newTimezone: "America/New_York");

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishPreferencesUpdatedAsync(
            @event,
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Combined Changes Tests

    [Fact]
    public async Task Handle_BothLanguageAndTimezoneChange_PublishesToRabbitMQ()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();

        var @event = new PreferencesUpdatedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldLanguage: "en",
            newLanguage: "de",
            oldTimezone: "UTC",
            newTimezone: "Europe/Berlin");

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishPreferencesUpdatedAsync(
            @event,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullValues_HandlesGracefully()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();

        var @event = new PreferencesUpdatedEvent(
            eventVersion: 1,
            profileId: profileId,
            userId: userId,
            oldLanguage: null,
            newLanguage: null,
            oldTimezone: null,
            newTimezone: null);

        // Act
        var act = () => _handler.Handle(@event, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        await _eventPublisher.Received(1).PublishPreferencesUpdatedAsync(
            @event,
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Event Data Verification Tests

    [Fact]
    public async Task Handle_PreservesEventData_InPublishedEvent()
    {
        // Arrange
        var profileId = UserProfileId.New();
        var userId = UserId.New();
        var oldLanguage = "en-US";
        var newLanguage = "de-DE";
        var oldTimezone = "UTC";
        var newTimezone = "Europe/Berlin";

        var @event = new PreferencesUpdatedEvent(
            eventVersion: 5,
            profileId: profileId,
            userId: userId,
            oldLanguage: oldLanguage,
            newLanguage: newLanguage,
            oldTimezone: oldTimezone,
            newTimezone: newTimezone);

        PreferencesUpdatedEvent? capturedEvent = null;
        await _eventPublisher.PublishPreferencesUpdatedAsync(
            Arg.Do<PreferencesUpdatedEvent>(e => capturedEvent = e),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.ProfileId.Should().Be(profileId);
        capturedEvent.UserId.Should().Be(userId);
        capturedEvent.OldLanguage.Should().Be(oldLanguage);
        capturedEvent.NewLanguage.Should().Be(newLanguage);
        capturedEvent.OldTimezone.Should().Be(oldTimezone);
        capturedEvent.NewTimezone.Should().Be(newTimezone);
    }

    #endregion
}
