using FamilyHub.Infrastructure.Messaging;
using FamilyHub.Modules.UserProfile.Application.Abstractions;
using FamilyHub.Modules.UserProfile.Domain.Events;
using FamilyHub.SharedKernel.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.EventHandlers;

/// <summary>
/// Handles DisplayNameChangedEvent to invalidate caches and send real-time updates.
/// Event chain: Profile → Cache invalidation + Real-time UI updates.
/// </summary>
/// <param name="cacheInvalidationService">Service for cache invalidation.</param>
/// <param name="calendarService">Calendar service for updating birthday event titles.</param>
/// <param name="userLookupService">User lookup service for FamilyId resolution.</param>
/// <param name="redisPublisher">Redis publisher for real-time GraphQL subscriptions.</param>
/// <param name="eventPublisher">Publisher for RabbitMQ event publishing.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class DisplayNameChangedEventHandler(
    ICacheInvalidationService cacheInvalidationService,
    ICalendarService calendarService,
    IUserLookupService userLookupService,
    IRedisSubscriptionPublisher redisPublisher,
    IProfileEventPublisher eventPublisher,
    ILogger<DisplayNameChangedEventHandler> logger)
    : INotificationHandler<DisplayNameChangedEvent>
{
    /// <inheritdoc />
    public async Task Handle(DisplayNameChangedEvent notification, CancellationToken cancellationToken)
    {
        LogHandlingDisplayNameChanged(
            notification.ProfileId.Value,
            notification.OldDisplayName.Value,
            notification.NewDisplayName.Value);

        // Resolve FamilyId for cache invalidation and real-time updates
        var familyId = await userLookupService.GetUserFamilyIdAsync(
            notification.UserId,
            cancellationToken);

        if (familyId is not null)
        {
            // Invalidate family members cache
            await cacheInvalidationService.InvalidateFamilyMembersCacheAsync(
                familyId.Value,
                cancellationToken);

            LogCacheInvalidated(familyId.Value.Value);

            // Update birthday calendar event title if exists
            try
            {
                await calendarService.UpdateBirthdayEventTitleAsync(
                    familyId.Value,
                    notification.UserId,
                    notification.NewDisplayName.Value,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                LogCalendarUpdateError(notification.UserId.Value, ex.Message);
            }

            // Send real-time update via Redis subscription
            await PublishRealtimeUpdateAsync(familyId.Value, notification, cancellationToken);
        }

        // Invalidate user profile cache
        await cacheInvalidationService.InvalidateUserProfileCacheAsync(
            notification.UserId,
            cancellationToken);

        // Publish to RabbitMQ for other consumers
        await eventPublisher.PublishDisplayNameChangedAsync(notification, cancellationToken);

        LogDisplayNameChangeHandled(notification.ProfileId.Value);
    }

    /// <summary>
    /// Publishes a real-time update via Redis for GraphQL subscriptions.
    /// </summary>
    private async Task PublishRealtimeUpdateAsync(
        SharedKernel.Domain.ValueObjects.FamilyId familyId,
        DisplayNameChangedEvent notification,
        CancellationToken cancellationToken)
    {
        var payload = new FamilyMemberUpdatedPayload(
            notification.UserId.Value,
            notification.NewDisplayName.Value);

        await redisPublisher.PublishAsync(
            $"family-member-updated:{familyId.Value}",
            payload,
            cancellationToken);

        LogRealtimeUpdatePublished(familyId.Value, notification.UserId.Value);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling DisplayNameChangedEvent for profile {ProfileId}: '{OldDisplayName}' -> '{NewDisplayName}'")]
    private partial void LogHandlingDisplayNameChanged(Guid profileId, string oldDisplayName, string newDisplayName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Family members cache invalidated for family {FamilyId}")]
    private partial void LogCacheInvalidated(Guid familyId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Calendar update error for user {UserId}: {ErrorMessage}")]
    private partial void LogCalendarUpdateError(Guid userId, string errorMessage);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Real-time update published for family {FamilyId}, user {UserId}")]
    private partial void LogRealtimeUpdatePublished(Guid familyId, Guid userId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "DisplayNameChangedEvent handled for profile {ProfileId}")]
    private partial void LogDisplayNameChangeHandled(Guid profileId);
}

/// <summary>
/// Payload for family member update real-time notifications.
/// </summary>
/// <param name="UserId">The user ID whose name changed.</param>
/// <param name="NewDisplayName">The new display name.</param>
internal sealed record FamilyMemberUpdatedPayload(Guid UserId, string NewDisplayName);
