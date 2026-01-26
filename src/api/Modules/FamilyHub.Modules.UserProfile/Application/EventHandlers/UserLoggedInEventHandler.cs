using FamilyHub.Modules.Auth.Domain.Events;
using FamilyHub.Modules.UserProfile.Application.Abstractions;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.Modules.UserProfile.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.EventHandlers;

/// <summary>
/// Handles UserLoggedInEvent to synchronize user profile data from Zitadel.
/// Creates a new profile if one doesn't exist, or syncs display name if Zitadel has newer data.
/// </summary>
/// <param name="profileRepository">Repository for user profiles.</param>
/// <param name="dbContext">Database context for persistence.</param>
/// <param name="zitadelSyncService">Service for Zitadel synchronization.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class UserLoggedInEventHandler(
    IUserProfileRepository profileRepository,
    UserProfileDbContext dbContext,
    IZitadelSyncService zitadelSyncService,
    ILogger<UserLoggedInEventHandler> logger)
    : INotificationHandler<UserLoggedInEvent>
{
    /// <inheritdoc />
    public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
    {
        LogHandlingUserLoggedIn(notification.UserId.Value, notification.ExternalUserId, notification.IsNewUser);

        // Check if profile exists
        var profile = await profileRepository.GetByUserIdAsync(notification.UserId, cancellationToken);

        if (profile == null)
        {
            // Create new profile from Zitadel data
            await CreateProfileFromZitadelAsync(notification, cancellationToken);
            return;
        }

        // Profile exists - sync from Zitadel if needed
        await SyncProfileFromZitadelAsync(profile, notification, cancellationToken);
    }

    /// <summary>
    /// Creates a new user profile using data from the Zitadel ID token.
    /// </summary>
    private async Task CreateProfileFromZitadelAsync(
        UserLoggedInEvent notification,
        CancellationToken cancellationToken)
    {
        // Use display name from Zitadel, or fall back to email prefix
        var displayNameValue = notification.DisplayNameFromProvider
                               ?? notification.Email.Value.Split('@')[0];

        LogCreatingProfileFromZitadel(notification.UserId.Value, displayNameValue);

        var displayName = DisplayName.From(displayNameValue);
        var profile = Domain.Aggregates.UserProfile.Create(notification.UserId, displayName);

        // Mark as synced since we just created from Zitadel data
        profile.MarkSynced();

        await profileRepository.AddAsync(profile, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        LogProfileCreatedFromZitadel(profile.Id.Value, displayNameValue);
    }

    /// <summary>
    /// Syncs an existing profile from Zitadel using last-write-wins strategy.
    /// </summary>
    private async Task SyncProfileFromZitadelAsync(
        Domain.Aggregates.UserProfile profile,
        UserLoggedInEvent notification,
        CancellationToken cancellationToken)
    {
        LogSyncingProfileFromZitadel(profile.Id.Value, notification.ExternalUserId);

        FamilyHub.SharedKernel.Domain.Result<SyncResult> syncResult = await zitadelSyncService.SyncFromZitadelAsync(
            profile,
            notification.ExternalUserId,
            cancellationToken);

        if (!syncResult.IsSuccess)
        {
            LogSyncFailed(profile.Id.Value, syncResult.Error);
            return;
        }

        if (syncResult.Value.WasUpdated)
        {
            // Update the profile with the display name from Zitadel
            profile.UpdateDisplayNameFromZitadel(syncResult.Value.DisplayName);
            profile.MarkSynced();

            await dbContext.SaveChangesAsync(cancellationToken);
            LogProfileUpdatedFromZitadel(profile.Id.Value, syncResult.Value.DisplayName.Value);
        }
        else
        {
            // Just mark as synced without changes
            profile.MarkSynced();
            await dbContext.SaveChangesAsync(cancellationToken);
            LogProfileSyncedNoChanges(profile.Id.Value);
        }
    }

    [LoggerMessage(LogLevel.Information, "Handling UserLoggedInEvent for user {UserId} (external: {ExternalUserId}, isNew: {IsNewUser})")]
    partial void LogHandlingUserLoggedIn(Guid userId, string externalUserId, bool isNewUser);

    [LoggerMessage(LogLevel.Information, "Creating new profile from Zitadel for user {UserId} with display name '{DisplayName}'")]
    partial void LogCreatingProfileFromZitadel(Guid userId, string displayName);

    [LoggerMessage(LogLevel.Information, "Profile {ProfileId} created from Zitadel with display name '{DisplayName}'")]
    partial void LogProfileCreatedFromZitadel(Guid profileId, string displayName);

    [LoggerMessage(LogLevel.Information, "Syncing profile {ProfileId} from Zitadel (external: {ExternalUserId})")]
    partial void LogSyncingProfileFromZitadel(Guid profileId, string externalUserId);

    [LoggerMessage(LogLevel.Warning, "Sync from Zitadel failed for profile {ProfileId}: {Error}")]
    partial void LogSyncFailed(Guid profileId, string error);

    [LoggerMessage(LogLevel.Information, "Profile {ProfileId} updated from Zitadel with display name '{DisplayName}'")]
    partial void LogProfileUpdatedFromZitadel(Guid profileId, string displayName);

    [LoggerMessage(LogLevel.Debug, "Profile {ProfileId} synced from Zitadel, no changes needed")]
    partial void LogProfileSyncedNoChanges(Guid profileId);
}
