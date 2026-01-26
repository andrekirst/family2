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
/// </summary>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="profileRepository">Repository for user profiles.</param>
/// <param name="dbContext">Database context for persistence.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class UpdateUserProfileCommandHandler(
    IUserContext userContext,
    IUserProfileRepository profileRepository,
    UserProfileDbContext dbContext,
    ILogger<UpdateUserProfileCommandHandler> logger)
    : ICommandHandler<UpdateUserProfileCommand, DomainResult>
{
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

        if (isNewProfile)
        {
            // Create new profile
            LogCreatingNewProfile(userId.Value, request.DisplayName.Value);
            profile = Domain.Aggregates.UserProfile.Create(userId, request.DisplayName);
            await profileRepository.AddAsync(profile, cancellationToken);
        }
        else
        {
            // Update existing profile (profile is guaranteed non-null in this branch)
            LogUpdatingExistingProfile(profile!.Id.Value, request.DisplayName.Value);
            profile.UpdateDisplayName(request.DisplayName);
        }

        // Update optional fields
        if (request.Birthday.HasValue)
        {
            profile.UpdateBirthday(request.Birthday);
        }

        if (request.Pronouns.HasValue)
        {
            profile.UpdatePronouns(request.Pronouns);
        }

        if (request.Preferences != null)
        {
            profile.UpdatePreferences(request.Preferences);
        }

        if (request.FieldVisibility != null)
        {
            profile.UpdateFieldVisibility(request.FieldVisibility);
        }

        // Persist changes
        await dbContext.SaveChangesAsync(cancellationToken);

        LogProfileUpdatedSuccessfully(profile.Id.Value);

        return DomainResult.Success(new UpdateUserProfileResult
        {
            ProfileId = profile.Id,
            DisplayName = profile.DisplayName,
            UpdatedAt = profile.UpdatedAt,
            IsNewProfile = isNewProfile
        });
    }

    [LoggerMessage(LogLevel.Information, "Updating profile for user {userId}")]
    partial void LogUpdatingProfileForUser(Guid userId);

    [LoggerMessage(LogLevel.Information, "Creating new profile for user {userId} with display name '{displayName}'")]
    partial void LogCreatingNewProfile(Guid userId, string displayName);

    [LoggerMessage(LogLevel.Information, "Updating existing profile {profileId} with display name '{displayName}'")]
    partial void LogUpdatingExistingProfile(Guid profileId, string displayName);

    [LoggerMessage(LogLevel.Information, "Profile {profileId} updated successfully")]
    partial void LogProfileUpdatedSuccessfully(Guid profileId);
}
