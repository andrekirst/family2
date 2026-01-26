using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetUserProfile;

/// <summary>
/// Handler for GetUserProfileQuery.
/// Returns another user's profile with fields filtered by visibility settings.
/// </summary>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="profileRepository">Repository for user profiles.</param>
/// <param name="userLookupService">Service for cross-module user lookups.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class GetUserProfileQueryHandler(
    IUserContext userContext,
    IUserProfileRepository profileRepository,
    IUserLookupService userLookupService,
    ILogger<GetUserProfileQueryHandler> logger)
    : IQueryHandler<GetUserProfileQuery, GetUserProfileResult?>
{
    /// <inheritdoc />
    public async Task<GetUserProfileResult?> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        var requestingUserId = userContext.UserId;
        var targetUserId = request.UserId;

        LogGettingProfileForTargetUser(targetUserId.Value, requestingUserId.Value);

        // Get the target user's profile
        var profile = await profileRepository.GetByUserIdAsync(targetUserId, cancellationToken);

        if (profile == null)
        {
            LogProfileNotFound(targetUserId.Value);
            return null;
        }

        // Determine if users are in the same family for visibility check
        var isSameFamily = await CheckSameFamilyAsync(requestingUserId, targetUserId, cancellationToken);
        LogVisibilityContext(profile.Id.Value, isSameFamily);

        // Apply visibility rules to each field
        var birthday = profile.IsFieldVisibleTo(nameof(profile.Birthday), requestingUserId, isSameFamily)
            ? profile.Birthday
            : null;

        var pronouns = profile.IsFieldVisibleTo(nameof(profile.Pronouns), requestingUserId, isSameFamily)
            ? profile.Pronouns
            : null;

        var preferences = profile.IsFieldVisibleTo(nameof(profile.Preferences), requestingUserId, isSameFamily)
            ? profile.Preferences
            : null;

        return new GetUserProfileResult
        {
            ProfileId = profile.Id,
            UserId = profile.UserId,
            DisplayName = profile.DisplayName, // Always visible
            Birthday = birthday,
            Pronouns = pronouns,
            Preferences = preferences,
            CreatedAt = profile.CreatedAt
        };
    }

    private async Task<bool> CheckSameFamilyAsync(
        SharedKernel.Domain.ValueObjects.UserId requestingUserId,
        SharedKernel.Domain.ValueObjects.UserId targetUserId,
        CancellationToken cancellationToken)
    {
        // If same user, treat as same family (owner always sees own profile fully)
        if (requestingUserId == targetUserId)
        {
            return true;
        }

        // Get family IDs for both users via cross-module lookup
        var requestingUserFamilyId = await userLookupService.GetUserFamilyIdAsync(requestingUserId, cancellationToken);
        var targetUserFamilyId = await userLookupService.GetUserFamilyIdAsync(targetUserId, cancellationToken);

        // Check if both have families and they're the same
        return requestingUserFamilyId != null &&
               targetUserFamilyId != null &&
               requestingUserFamilyId == targetUserFamilyId;
    }

    [LoggerMessage(LogLevel.Debug, "Getting profile for user {targetUserId} requested by {requestingUserId}")]
    partial void LogGettingProfileForTargetUser(Guid targetUserId, Guid requestingUserId);

    [LoggerMessage(LogLevel.Debug, "Profile not found for user {userId}")]
    partial void LogProfileNotFound(Guid userId);

    [LoggerMessage(LogLevel.Debug, "Profile {profileId} visibility context: isSameFamily={isSameFamily}")]
    partial void LogVisibilityContext(Guid profileId, bool isSameFamily);
}
