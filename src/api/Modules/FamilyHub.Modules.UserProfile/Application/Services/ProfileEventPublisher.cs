using FamilyHub.Modules.UserProfile.Application.Abstractions;
using FamilyHub.Modules.UserProfile.Domain.Events;
using FamilyHub.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.Services;

/// <summary>
/// Publishes profile events to RabbitMQ for cross-module consumption.
/// Events are published to the 'profile.events' exchange with type-specific routing keys.
/// </summary>
/// <remarks>
/// <para>Exchange: profile.events (topic)</para>
/// <para>Routing keys:</para>
/// <list type="bullet">
///   <item>profile.birthday.set - Birthday events</item>
///   <item>profile.name.changed - Display name change events</item>
///   <item>profile.preferences.updated - Preferences change events</item>
/// </list>
/// </remarks>
public sealed partial class ProfileEventPublisher : IProfileEventPublisher
{
    /// <summary>
    /// The exchange name for profile events.
    /// </summary>
    private const string ExchangeName = "profile.events";

    /// <summary>
    /// Routing key for birthday set events.
    /// </summary>
    private const string BirthdaySetRoutingKey = "profile.birthday.set";

    /// <summary>
    /// Routing key for display name changed events.
    /// </summary>
    private const string DisplayNameChangedRoutingKey = "profile.name.changed";

    /// <summary>
    /// Routing key for preferences updated events.
    /// </summary>
    private const string PreferencesUpdatedRoutingKey = "profile.preferences.updated";

    private readonly IMessageBrokerPublisher _publisher;
    private readonly ILogger<ProfileEventPublisher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileEventPublisher"/> class.
    /// </summary>
    /// <param name="publisher">The message broker publisher for RabbitMQ.</param>
    /// <param name="logger">Logger for structured logging.</param>
    public ProfileEventPublisher(
        IMessageBrokerPublisher publisher,
        ILogger<ProfileEventPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishBirthdaySetAsync(
        BirthdaySetEvent @event,
        CancellationToken cancellationToken = default)
    {
        LogPublishingEvent(nameof(BirthdaySetEvent), @event.ProfileId.Value, BirthdaySetRoutingKey);

        // Create integration event payload with serializable types
        var payload = new BirthdaySetIntegrationEvent(
            EventId: @event.EventId,
            ProfileId: @event.ProfileId.Value,
            UserId: @event.UserId.Value,
            Birthday: @event.Birthday.Value,
            DisplayName: @event.DisplayName.Value,
            OccurredAt: @event.OccurredOn);

        await _publisher.PublishAsync(
            ExchangeName,
            BirthdaySetRoutingKey,
            payload,
            cancellationToken);

        LogEventPublished(nameof(BirthdaySetEvent), @event.ProfileId.Value);
    }

    /// <inheritdoc />
    public async Task PublishDisplayNameChangedAsync(
        DisplayNameChangedEvent @event,
        CancellationToken cancellationToken = default)
    {
        LogPublishingEvent(nameof(DisplayNameChangedEvent), @event.ProfileId.Value, DisplayNameChangedRoutingKey);

        var payload = new DisplayNameChangedIntegrationEvent(
            EventId: @event.EventId,
            ProfileId: @event.ProfileId.Value,
            UserId: @event.UserId.Value,
            OldDisplayName: @event.OldDisplayName.Value,
            NewDisplayName: @event.NewDisplayName.Value,
            OccurredAt: @event.OccurredOn);

        await _publisher.PublishAsync(
            ExchangeName,
            DisplayNameChangedRoutingKey,
            payload,
            cancellationToken);

        LogEventPublished(nameof(DisplayNameChangedEvent), @event.ProfileId.Value);
    }

    /// <inheritdoc />
    public async Task PublishPreferencesUpdatedAsync(
        PreferencesUpdatedEvent @event,
        CancellationToken cancellationToken = default)
    {
        LogPublishingEvent(nameof(PreferencesUpdatedEvent), @event.ProfileId.Value, PreferencesUpdatedRoutingKey);

        var payload = new PreferencesUpdatedIntegrationEvent(
            EventId: @event.EventId,
            ProfileId: @event.ProfileId.Value,
            UserId: @event.UserId.Value,
            OldLanguage: @event.OldLanguage,
            NewLanguage: @event.NewLanguage,
            OldTimezone: @event.OldTimezone,
            NewTimezone: @event.NewTimezone,
            OccurredAt: @event.OccurredOn);

        await _publisher.PublishAsync(
            ExchangeName,
            PreferencesUpdatedRoutingKey,
            payload,
            cancellationToken);

        LogEventPublished(nameof(PreferencesUpdatedEvent), @event.ProfileId.Value);
    }

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Publishing {EventType} for profile {ProfileId} to routing key '{RoutingKey}'")]
    private partial void LogPublishingEvent(string eventType, Guid profileId, string routingKey);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{EventType} published for profile {ProfileId}")]
    private partial void LogEventPublished(string eventType, Guid profileId);
}

/// <summary>
/// Integration event payload for birthday set events.
/// Uses primitive types for JSON serialization compatibility.
/// </summary>
internal sealed record BirthdaySetIntegrationEvent(
    Guid EventId,
    Guid ProfileId,
    Guid UserId,
    DateOnly Birthday,
    string DisplayName,
    DateTime OccurredAt);

/// <summary>
/// Integration event payload for display name changed events.
/// Uses primitive types for JSON serialization compatibility.
/// </summary>
internal sealed record DisplayNameChangedIntegrationEvent(
    Guid EventId,
    Guid ProfileId,
    Guid UserId,
    string OldDisplayName,
    string NewDisplayName,
    DateTime OccurredAt);

/// <summary>
/// Integration event payload for preferences updated events.
/// Uses primitive types for JSON serialization compatibility.
/// </summary>
internal sealed record PreferencesUpdatedIntegrationEvent(
    Guid EventId,
    Guid ProfileId,
    Guid UserId,
    string? OldLanguage,
    string? NewLanguage,
    string? OldTimezone,
    string? NewTimezone,
    DateTime OccurredAt);
