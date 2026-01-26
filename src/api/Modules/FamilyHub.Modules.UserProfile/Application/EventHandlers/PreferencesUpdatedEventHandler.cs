using FamilyHub.Modules.UserProfile.Application.Abstractions;
using FamilyHub.Modules.UserProfile.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.EventHandlers;

/// <summary>
/// Handles PreferencesUpdatedEvent for future Notifications module integration.
/// Currently a stub that logs the event - will be enhanced when Notifications module is implemented.
/// Event chain: Profile → Notifications (future).
/// </summary>
/// <param name="eventPublisher">Publisher for RabbitMQ event publishing.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class PreferencesUpdatedEventHandler(
    IProfileEventPublisher eventPublisher,
    ILogger<PreferencesUpdatedEventHandler> logger)
    : INotificationHandler<PreferencesUpdatedEvent>
{
    /// <inheritdoc />
    public async Task Handle(PreferencesUpdatedEvent notification, CancellationToken cancellationToken)
    {
        LogHandlingPreferencesUpdated(
            notification.ProfileId.Value,
            notification.UserId.Value);

        // Log preference changes for debugging
        if (notification.OldLanguage != notification.NewLanguage)
        {
            LogLanguageChanged(
                notification.UserId.Value,
                notification.OldLanguage ?? "(none)",
                notification.NewLanguage ?? "(none)");
        }

        if (notification.OldTimezone != notification.NewTimezone)
        {
            LogTimezoneChanged(
                notification.UserId.Value,
                notification.OldTimezone ?? "(none)",
                notification.NewTimezone ?? "(none)");
        }

        // Stub for future Notifications module integration
        // TODO: When Notifications module is implemented:
        // - Update notification delivery preferences
        // - Adjust notification timing based on timezone
        // - Update notification language templates
        LogNotificationsIntegrationPending(notification.UserId.Value);

        // Publish to RabbitMQ for other consumers
        await eventPublisher.PublishPreferencesUpdatedAsync(notification, cancellationToken);

        LogPreferencesUpdateHandled(notification.ProfileId.Value);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling PreferencesUpdatedEvent for profile {ProfileId}, user {UserId}")]
    private partial void LogHandlingPreferencesUpdated(Guid profileId, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Language preference changed for user {UserId}: '{OldLanguage}' -> '{NewLanguage}'")]
    private partial void LogLanguageChanged(Guid userId, string oldLanguage, string newLanguage);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Timezone preference changed for user {UserId}: '{OldTimezone}' -> '{NewTimezone}'")]
    private partial void LogTimezoneChanged(Guid userId, string oldTimezone, string newTimezone);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Notifications module integration pending for user {UserId} - preferences updated will be processed when module is available")]
    private partial void LogNotificationsIntegrationPending(Guid userId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "PreferencesUpdatedEvent handled for profile {ProfileId}")]
    private partial void LogPreferencesUpdateHandled(Guid profileId);
}
