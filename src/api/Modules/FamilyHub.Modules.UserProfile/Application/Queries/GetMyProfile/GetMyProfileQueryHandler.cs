using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetMyProfile;

/// <summary>
/// Handler for GetMyProfileQuery.
/// Returns the current user's profile with all fields visible.
/// </summary>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="profileRepository">Repository for user profiles.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class GetMyProfileQueryHandler(
    IUserContext userContext,
    IUserProfileRepository profileRepository,
    ILogger<GetMyProfileQueryHandler> logger)
    : IQueryHandler<GetMyProfileQuery, GetMyProfileResult?>
{
    /// <inheritdoc />
    public async Task<GetMyProfileResult?> Handle(
        GetMyProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        LogGettingProfileForUser(userId.Value);

        var profile = await profileRepository.GetByUserIdAsync(userId, cancellationToken);

        if (profile == null)
        {
            LogProfileNotFound(userId.Value);
            return null;
        }

        LogProfileFound(profile.Id.Value);

        return new GetMyProfileResult
        {
            ProfileId = profile.Id,
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            Birthday = profile.Birthday,
            Pronouns = profile.Pronouns,
            Preferences = profile.Preferences,
            FieldVisibility = profile.FieldVisibility,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }

    [LoggerMessage(LogLevel.Debug, "Getting profile for user {userId}")]
    partial void LogGettingProfileForUser(Guid userId);

    [LoggerMessage(LogLevel.Debug, "Profile not found for user {userId}")]
    partial void LogProfileNotFound(Guid userId);

    [LoggerMessage(LogLevel.Debug, "Found profile {profileId}")]
    partial void LogProfileFound(Guid profileId);
}
