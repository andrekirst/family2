using FamilyHub.Modules.UserProfile.Domain.Events;

namespace FamilyHub.Modules.UserProfile.Application.Abstractions;

/// <summary>
/// Service interface for publishing profile events to RabbitMQ for cross-module consumption.
/// Events are published with FamilyId (enriched from IUserLookupService) for routing.
/// </summary>
public interface IProfileEventPublisher
{
    /// <summary>
    /// Publishes a BirthdaySetEvent to RabbitMQ.
    /// Routing key: profile.birthday.set
    /// </summary>
    Task PublishBirthdaySetAsync(
        BirthdaySetEvent @event,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a DisplayNameChangedEvent to RabbitMQ.
    /// Routing key: profile.name.changed
    /// </summary>
    Task PublishDisplayNameChangedAsync(
        DisplayNameChangedEvent @event,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a PreferencesUpdatedEvent to RabbitMQ.
    /// Routing key: profile.preferences.updated
    /// </summary>
    Task PublishPreferencesUpdatedAsync(
        PreferencesUpdatedEvent @event,
        CancellationToken cancellationToken = default);
}
