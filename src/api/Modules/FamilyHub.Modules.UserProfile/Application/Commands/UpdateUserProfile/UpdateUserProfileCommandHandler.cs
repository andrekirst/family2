using System.Text.Json;
using FamilyHub.Modules.UserProfile.Application.Abstractions;
using FamilyHub.Modules.UserProfile.Application.Services.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Persistence;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using Microsoft.Extensions.Logging;
using DomainResult = FamilyHub.SharedKernel.Domain.Result<FamilyHub.Modules.UserProfile.Application.Commands.UpdateUserProfile.UpdateUserProfileResult>;

namespace FamilyHub.Modules.UserProfile.Application.Commands.UpdateUserProfile;

/// <summary>
/// Handler for UpdateUserProfileCommand.
/// Creates a new profile if one doesn't exist, or updates the existing profile.
/// Records all changes as events for audit trail.
/// Triggers Zitadel sync when display name changes.
/// </summary>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="profileRepository">Repository for user profiles.</param>
/// <param name="dbContext">Database context for persistence.</param>
/// <param name="eventRecorder">Service for recording profile change events.</param>
/// <param name="zitadelSyncService">Service for synchronizing with Zitadel.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class UpdateUserProfileCommandHandler(
    IUserContext userContext,
    IUserProfileRepository profileRepository,
    UserProfileDbContext dbContext,
    IProfileEventRecorder eventRecorder,
    IZitadelSyncService zitadelSyncService,
    ILogger<UpdateUserProfileCommandHandler> logger)
    : ICommandHandler<UpdateUserProfileCommand, DomainResult>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    /// <inheritdoc />
    public async Task<DomainResult> Handle(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        LogUpdatingProfileForUser(userId.Value);

        // Try to find existing profile
        var profile = await profileRepository.GetByUserIdAsync(userId, cancellationToken);
        var isNewProfile = profile == null;
        var displayNameChanged = false;

        if (isNewProfile)
        {
            // Create new profile
            LogCreatingNewProfile(userId.Value, request.DisplayName.Value);
            profile = Domain.Aggregates.UserProfile.Create(userId, request.DisplayName);
            await profileRepository.AddAsync(profile, cancellationToken);

            // Record creation event for audit trail
            await eventRecorder.RecordCreatedAsync(profile, userId, cancellationToken);

            // New profile needs to sync to Zitadel
            displayNameChanged = true;
        }
        else
        {
            // Ensure events exist for backward compatibility (creates synthetic event if needed)
            await eventRecorder.EnsureEventsExistAsync(profile!, userId, cancellationToken);

            // Track if display name is changing for Zitadel sync
            displayNameChanged = profile!.DisplayName != request.DisplayName;

            // Update existing profile with event recording
            LogUpdatingExistingProfile(profile.Id.Value, request.DisplayName.Value);
            await UpdateDisplayNameWithEvent(profile, request.DisplayName, userId, cancellationToken);
        }

        // Update optional fields with event recording
        if (request.Birthday.HasValue)
        {
            await UpdateBirthdayWithEvent(profile, request.Birthday, userId, cancellationToken);
        }

        if (request.Pronouns.HasValue)
        {
            await UpdatePronounsWithEvent(profile, request.Pronouns, userId, cancellationToken);
        }

        if (request.Preferences != null)
        {
            await UpdatePreferencesWithEvent(profile, request.Preferences, userId, cancellationToken);
        }

        if (request.FieldVisibility != null)
        {
            await UpdateFieldVisibilityWithEvent(profile, request.FieldVisibility, userId, cancellationToken);
        }

        // Persist changes (both state and events are saved atomically)
        await dbContext.SaveChangesAsync(cancellationToken);

        LogProfileUpdatedSuccessfully(profile.Id.Value);

        // Trigger Zitadel sync if display name changed
        if (displayNameChanged)
        {
            await SyncDisplayNameToZitadelAsync(profile, userId, cancellationToken);
        }

        return DomainResult.Success(new UpdateUserProfileResult
        {
            ProfileId = profile.Id,
            DisplayName = profile.DisplayName,
            UpdatedAt = profile.UpdatedAt,
            IsNewProfile = isNewProfile
        });
    }

    /// <summary>
    /// Syncs the display name to Zitadel asynchronously.
    /// Updates the profile's sync status based on the result.
    /// </summary>
    private async Task SyncDisplayNameToZitadelAsync(
        Domain.Aggregates.UserProfile profile,
        SharedKernel.Domain.ValueObjects.UserId userId,
        CancellationToken cancellationToken)
    {
        LogSyncingToZitadel(profile.Id.Value, profile.DisplayName.Value);

        var syncResult = await zitadelSyncService.PushDisplayNameAsync(
            userId,
            profile.DisplayName,
            cancellationToken);

        if (syncResult.IsSuccess)
        {
            profile.MarkSynced();
            LogZitadelSyncSucceeded(profile.Id.Value);
        }
        else
        {
            profile.MarkSyncFailed();
            LogZitadelSyncFailed(profile.Id.Value, syncResult.Error);
        }

        // Save sync status update
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateDisplayNameWithEvent(
        Domain.Aggregates.UserProfile profile,
        Domain.ValueObjects.DisplayName newDisplayName,
        SharedKernel.Domain.ValueObjects.UserId changedBy,
        CancellationToken cancellationToken)
    {
        if (profile.DisplayName == newDisplayName)
        {
            return;
        }

        var oldValue = profile.DisplayName.Value;
        profile.UpdateDisplayName(newDisplayName);

        await eventRecorder.RecordFieldUpdateAsync(
            profile.Id,
            changedBy,
            nameof(ProfileStateDto.DisplayName),
            oldValue,
            newDisplayName.Value,
            cancellationToken);
    }

    private async Task UpdateBirthdayWithEvent(
        Domain.Aggregates.UserProfile profile,
        Domain.ValueObjects.Birthday? newBirthday,
        SharedKernel.Domain.ValueObjects.UserId changedBy,
        CancellationToken cancellationToken)
    {
        if (profile.Birthday == newBirthday)
        {
            return;
        }

        var oldValue = profile.Birthday?.Value.ToString("yyyy-MM-dd");
        profile.UpdateBirthday(newBirthday);

        await eventRecorder.RecordFieldUpdateAsync(
            profile.Id,
            changedBy,
            nameof(ProfileStateDto.Birthday),
            oldValue,
            newBirthday?.Value.ToString("yyyy-MM-dd"),
            cancellationToken);
    }

    private async Task UpdatePronounsWithEvent(
        Domain.Aggregates.UserProfile profile,
        Domain.ValueObjects.Pronouns? newPronouns,
        SharedKernel.Domain.ValueObjects.UserId changedBy,
        CancellationToken cancellationToken)
    {
        if (profile.Pronouns == newPronouns)
        {
            return;
        }

        var oldValue = profile.Pronouns?.Value;
        profile.UpdatePronouns(newPronouns);

        await eventRecorder.RecordFieldUpdateAsync(
            profile.Id,
            changedBy,
            nameof(ProfileStateDto.Pronouns),
            oldValue,
            newPronouns?.Value,
            cancellationToken);
    }

    private async Task UpdatePreferencesWithEvent(
        Domain.Aggregates.UserProfile profile,
        Domain.ValueObjects.ProfilePreferences newPreferences,
        SharedKernel.Domain.ValueObjects.UserId changedBy,
        CancellationToken cancellationToken)
    {
        var oldPreferences = profile.Preferences;
        profile.UpdatePreferences(newPreferences);

        // Serialize preferences for comparison and storage
        var oldJson = SerializePreferences(oldPreferences);
        var newJson = SerializePreferences(newPreferences);

        if (oldJson != newJson)
        {
            await eventRecorder.RecordFieldUpdateAsync(
                profile.Id,
                changedBy,
                "Preferences",
                oldJson,
                newJson,
                cancellationToken);
        }
    }

    private async Task UpdateFieldVisibilityWithEvent(
        Domain.Aggregates.UserProfile profile,
        Domain.ValueObjects.ProfileFieldVisibility newVisibility,
        SharedKernel.Domain.ValueObjects.UserId changedBy,
        CancellationToken cancellationToken)
    {
        var oldVisibility = profile.FieldVisibility;
        profile.UpdateFieldVisibility(newVisibility);

        // Serialize visibility for comparison and storage
        var oldJson = SerializeFieldVisibility(oldVisibility);
        var newJson = SerializeFieldVisibility(newVisibility);

        if (oldJson != newJson)
        {
            await eventRecorder.RecordFieldUpdateAsync(
                profile.Id,
                changedBy,
                "FieldVisibility",
                oldJson,
                newJson,
                cancellationToken);
        }
    }

    private static string? SerializePreferences(Domain.ValueObjects.ProfilePreferences? preferences)
    {
        if (preferences == null)
        {
            return null;
        }

        return JsonSerializer.Serialize(new
        {
            language = preferences.Language,
            timezone = preferences.Timezone,
            dateFormat = preferences.DateFormat
        }, JsonOptions);
    }

    private static string? SerializeFieldVisibility(Domain.ValueObjects.ProfileFieldVisibility? visibility)
    {
        if (visibility == null)
        {
            return null;
        }

        return JsonSerializer.Serialize(new
        {
            birthdayVisibility = visibility.BirthdayVisibility.Value,
            pronounsVisibility = visibility.PronounsVisibility.Value,
            preferencesVisibility = visibility.PreferencesVisibility.Value
        }, JsonOptions);
    }

    [LoggerMessage(LogLevel.Information, "Updating profile for user {userId}")]
    partial void LogUpdatingProfileForUser(Guid userId);

    [LoggerMessage(LogLevel.Information, "Creating new profile for user {userId} with display name '{displayName}'")]
    partial void LogCreatingNewProfile(Guid userId, string displayName);

    [LoggerMessage(LogLevel.Information, "Updating existing profile {profileId} with display name '{displayName}'")]
    partial void LogUpdatingExistingProfile(Guid profileId, string displayName);

    [LoggerMessage(LogLevel.Information, "Profile {profileId} updated successfully")]
    partial void LogProfileUpdatedSuccessfully(Guid profileId);

    [LoggerMessage(LogLevel.Information, "Syncing display name to Zitadel for profile {profileId}: {displayName}")]
    partial void LogSyncingToZitadel(Guid profileId, string displayName);

    [LoggerMessage(LogLevel.Information, "Zitadel sync succeeded for profile {profileId}")]
    partial void LogZitadelSyncSucceeded(Guid profileId);

    [LoggerMessage(LogLevel.Warning, "Zitadel sync failed for profile {profileId}: {error}")]
    partial void LogZitadelSyncFailed(Guid profileId, string error);
}
