using FamilyHub.Modules.UserProfile.Application.Abstractions;
using FamilyHub.Modules.UserProfile.Domain.Events;
using FamilyHub.SharedKernel.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.EventHandlers;

/// <summary>
/// Handles BirthdaySetEvent to create recurring birthday calendar events.
/// Event chain: Profile → Calendar integration.
/// </summary>
/// <param name="calendarService">Calendar service for creating birthday events.</param>
/// <param name="userLookupService">User lookup service for FamilyId resolution.</param>
/// <param name="eventPublisher">Publisher for RabbitMQ event publishing.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class BirthdaySetEventHandler(
    ICalendarService calendarService,
    IUserLookupService userLookupService,
    IProfileEventPublisher eventPublisher,
    ILogger<BirthdaySetEventHandler> logger)
    : INotificationHandler<BirthdaySetEvent>
{
    /// <inheritdoc />
    public async Task Handle(BirthdaySetEvent notification, CancellationToken cancellationToken)
    {
        LogHandlingBirthdaySet(
            notification.ProfileId.Value,
            notification.UserId.Value,
            notification.Birthday.Value);

        // Resolve FamilyId for cross-module operations
        var familyId = await userLookupService.GetUserFamilyIdAsync(
            notification.UserId,
            cancellationToken);

        if (familyId is null)
        {
            LogUserNotInFamily(notification.UserId.Value);
            // User not in a family - skip calendar event creation but still publish for other consumers
            await eventPublisher.PublishBirthdaySetAsync(notification, cancellationToken);
            return;
        }

        // Create recurring birthday event in calendar
        try
        {
            await calendarService.CreateRecurringBirthdayEventAsync(
                familyId.Value,
                notification.UserId,
                notification.DisplayName.Value,
                notification.Birthday.Value,
                cancellationToken);

            LogBirthdayEventCreated(
                familyId.Value.Value,
                notification.UserId.Value,
                notification.DisplayName.Value);
        }
        catch (Exception ex)
        {
            // Log but don't fail - calendar is a side effect
            LogCalendarServiceError(notification.UserId.Value, ex.Message);
        }

        // Publish to RabbitMQ for other consumers
        await eventPublisher.PublishBirthdaySetAsync(notification, cancellationToken);

        LogBirthdaySetEventHandled(notification.ProfileId.Value);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling BirthdaySetEvent for profile {ProfileId}, user {UserId}, birthday {Birthday}")]
    private partial void LogHandlingBirthdaySet(Guid profileId, Guid userId, DateOnly birthday);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User {UserId} is not in a family - skipping calendar event creation")]
    private partial void LogUserNotInFamily(Guid userId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Created birthday calendar event for family {FamilyId}, user {UserId}, name '{DisplayName}'")]
    private partial void LogBirthdayEventCreated(Guid familyId, Guid userId, string displayName);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Calendar service error for user {UserId}: {ErrorMessage}")]
    private partial void LogCalendarServiceError(Guid userId, string errorMessage);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "BirthdaySetEvent handled for profile {ProfileId}")]
    private partial void LogBirthdaySetEventHandled(Guid profileId);
}
