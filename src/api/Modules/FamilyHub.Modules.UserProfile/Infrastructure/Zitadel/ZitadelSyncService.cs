using FamilyHub.Modules.UserProfile.Application.Abstractions;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

// Alias to disambiguate from GreenDonut.Result<T>
using Result = FamilyHub.SharedKernel.Domain.Result;

namespace FamilyHub.Modules.UserProfile.Infrastructure.Zitadel;

/// <summary>
/// Service for synchronizing user profile data with Zitadel identity provider.
/// Implements bidirectional sync with last-write-wins conflict resolution.
/// </summary>
public sealed partial class ZitadelSyncService : IZitadelSyncService
{
    private readonly IZitadelManagementApiClient _zitadelClient;
    private readonly IUserLookupService _userLookupService;
    private readonly ILogger<ZitadelSyncService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZitadelSyncService"/> class.
    /// </summary>
    public ZitadelSyncService(
        IZitadelManagementApiClient zitadelClient,
        IUserLookupService userLookupService,
        ILogger<ZitadelSyncService> logger)
    {
        _zitadelClient = zitadelClient;
        _userLookupService = userLookupService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> PushDisplayNameAsync(
        UserId userId,
        DisplayName displayName,
        CancellationToken cancellationToken = default)
    {
        LogPushingDisplayName(userId.Value, displayName.Value);

        // Get the external user ID from the Auth module
        var externalUserId = await _userLookupService.GetExternalUserIdAsync(userId, cancellationToken);
        if (string.IsNullOrEmpty(externalUserId))
        {
            LogExternalUserIdNotFound(userId.Value);
            return Result.Failure("External user ID not found. User may not be linked to Zitadel.");
        }

        // Push to Zitadel
        var success = await _zitadelClient.UpdateUserProfileAsync(
            externalUserId,
            displayName.Value,
            cancellationToken);

        if (!success)
        {
            LogPushFailed(userId.Value, externalUserId);
            return Result.Failure("Failed to update display name in Zitadel.");
        }

        LogPushSucceeded(userId.Value, externalUserId, displayName.Value);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<DisplayName?> PullDisplayNameAsync(
        string externalUserId,
        CancellationToken cancellationToken = default)
    {
        LogPullingDisplayName(externalUserId);

        var zitadelProfile = await _zitadelClient.GetUserProfileAsync(externalUserId, cancellationToken);
        if (zitadelProfile == null)
        {
            LogZitadelProfileNotFound(externalUserId);
            return null;
        }

        try
        {
            var displayName = DisplayName.From(zitadelProfile.DisplayName);
            LogPullSucceeded(externalUserId, displayName.Value);
            return displayName;
        }
        catch (Exception ex)
        {
            LogDisplayNameValidationFailed(externalUserId, zitadelProfile.DisplayName, ex);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<SyncResult>> SyncFromZitadelAsync(
        Domain.Aggregates.UserProfile profile,
        string externalUserId,
        CancellationToken cancellationToken = default)
    {
        LogSyncingFromZitadel(profile.Id.Value, externalUserId);

        var zitadelProfile = await _zitadelClient.GetUserProfileAsync(externalUserId, cancellationToken);
        if (zitadelProfile == null)
        {
            LogZitadelProfileNotFoundForSync(profile.Id.Value, externalUserId);
            // Mark as synced anyway - Zitadel doesn't have the profile, local is source of truth
            return Result.Success(SyncResult.NoUpdate(profile.DisplayName));
        }

        // Apply last-write-wins logic
        // For now, we always prefer local changes unless this is first sync
        // In the future, we could compare timestamps if Zitadel provides them
        var isFirstSync = profile.LastSyncedAt == null;

        if (isFirstSync)
        {
            // On first sync during login, prefer Zitadel's data
            // (user may have updated their profile in Zitadel before we created local profile)
            try
            {
                var zitadelDisplayName = DisplayName.From(zitadelProfile.DisplayName);

                if (profile.DisplayName != zitadelDisplayName)
                {
                    LogUpdatingFromZitadel(profile.Id.Value, profile.DisplayName.Value, zitadelDisplayName.Value);
                    return Result.Success(SyncResult.Updated(zitadelDisplayName));
                }
            }
            catch (Exception ex)
            {
                LogDisplayNameValidationFailed(externalUserId, zitadelProfile.DisplayName, ex);
                // Continue with local data
            }
        }

        LogSyncCompleteNoChanges(profile.Id.Value);
        return Result.Success(SyncResult.NoUpdate(profile.DisplayName));
    }

    [LoggerMessage(LogLevel.Information, "Pushing display name to Zitadel for user {UserId}: {DisplayName}")]
    partial void LogPushingDisplayName(Guid userId, string displayName);

    [LoggerMessage(LogLevel.Warning, "External user ID not found for user {UserId}")]
    partial void LogExternalUserIdNotFound(Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to push display name to Zitadel for user {UserId} (external: {ExternalUserId})")]
    partial void LogPushFailed(Guid userId, string externalUserId);

    [LoggerMessage(LogLevel.Information, "Successfully pushed display name to Zitadel for user {UserId} (external: {ExternalUserId}): {DisplayName}")]
    partial void LogPushSucceeded(Guid userId, string externalUserId, string displayName);

    [LoggerMessage(LogLevel.Information, "Pulling display name from Zitadel for external user {ExternalUserId}")]
    partial void LogPullingDisplayName(string externalUserId);

    [LoggerMessage(LogLevel.Warning, "Zitadel profile not found for external user {ExternalUserId}")]
    partial void LogZitadelProfileNotFound(string externalUserId);

    [LoggerMessage(LogLevel.Debug, "Successfully pulled display name from Zitadel for external user {ExternalUserId}: {DisplayName}")]
    partial void LogPullSucceeded(string externalUserId, string displayName);

    [LoggerMessage(LogLevel.Warning, "Display name validation failed for external user {ExternalUserId}: {DisplayName}")]
    partial void LogDisplayNameValidationFailed(string externalUserId, string displayName, Exception exception);

    [LoggerMessage(LogLevel.Information, "Syncing profile from Zitadel for profile {ProfileId} (external: {ExternalUserId})")]
    partial void LogSyncingFromZitadel(Guid profileId, string externalUserId);

    [LoggerMessage(LogLevel.Warning, "Zitadel profile not found during sync for profile {ProfileId} (external: {ExternalUserId})")]
    partial void LogZitadelProfileNotFoundForSync(Guid profileId, string externalUserId);

    [LoggerMessage(LogLevel.Information, "Updating profile {ProfileId} from Zitadel: {OldDisplayName} -> {NewDisplayName}")]
    partial void LogUpdatingFromZitadel(Guid profileId, string oldDisplayName, string newDisplayName);

    [LoggerMessage(LogLevel.Debug, "Sync complete for profile {ProfileId}, no changes needed")]
    partial void LogSyncCompleteNoChanges(Guid profileId);
}
