using System.Text.Json;
using FamilyHub.Modules.UserProfile.Application.Services.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.Aggregates;
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
/// For child users, changes require parent approval instead of being applied directly.
/// </summary>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="profileRepository">Repository for user profiles.</param>
/// <param name="changeRequestRepository">Repository for profile change requests.</param>
/// <param name="dbContext">Database context for persistence.</param>
/// <param name="eventRecorder">Service for recording profile change events.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class UpdateUserProfileCommandHandler(
    IUserContext userContext,
    IUserProfileRepository profileRepository,
    IProfileChangeRequestRepository changeRequestRepository,
    UserProfileDbContext dbContext,
    IProfileEventRecorder eventRecorder,
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

        // Child users' profile changes require parent approval
        if (userContext.IsChild)
        {
            LogChildUserProfileUpdate(userId.Value);
            return await HandleChildProfileUpdateAsync(request, cancellationToken);
        }

        // Try to find existing profile
        var profile = await profileRepository.GetByUserIdAsync(userId, cancellationToken);
        var isNewProfile = profile == null;

        if (isNewProfile)
        {
            // Create new profile
            LogCreatingNewProfile(userId.Value, request.DisplayName.Value);
            profile = Domain.Aggregates.UserProfile.Create(userId, request.DisplayName);
            await profileRepository.AddAsync(profile, cancellationToken);

            // Record creation event for audit trail
            await eventRecorder.RecordCreatedAsync(profile, userId, cancellationToken);
        }
        else
        {
            // Ensure events exist for backward compatibility (creates synthetic event if needed)
            await eventRecorder.EnsureEventsExistAsync(profile!, userId, cancellationToken);

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

        return DomainResult.Success(new UpdateUserProfileResult
        {
            ProfileId = profile.Id,
            DisplayName = profile.DisplayName,
            UpdatedAt = profile.UpdatedAt,
            IsNewProfile = isNewProfile
        });
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

    [LoggerMessage(LogLevel.Information, "Child user {userId} requesting profile update - changes require parent approval")]
    partial void LogChildUserProfileUpdate(Guid userId);

    [LoggerMessage(LogLevel.Information, "Created {count} change request(s) for child profile {profileId}")]
    partial void LogChangeRequestsCreated(int count, Guid profileId);

    /// <summary>
    /// Handles profile updates for child users.
    /// Instead of applying changes directly, creates change requests that require parent approval.
    /// </summary>
    private async Task<DomainResult> HandleChildProfileUpdateAsync(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var familyId = userContext.FamilyId;

        // Get or create the profile (child must have a profile to request changes)
        var profile = await profileRepository.GetByUserIdAsync(userId, cancellationToken);

        if (profile == null)
        {
            // Child's first profile creation - this is allowed without approval
            // The initial profile is just a placeholder, parents can review later
            profile = Domain.Aggregates.UserProfile.Create(userId, request.DisplayName);
            await profileRepository.AddAsync(profile, cancellationToken);
            await eventRecorder.RecordCreatedAsync(profile, userId, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return DomainResult.Success(new UpdateUserProfileResult
            {
                ProfileId = profile.Id,
                DisplayName = profile.DisplayName,
                UpdatedAt = profile.UpdatedAt,
                IsNewProfile = true,
                RequiresApproval = false,
                PendingChangesCount = 0
            });
        }

        // Create change requests for each modified field
        var changeRequests = new List<ProfileChangeRequest>();

        // Display Name change
        if (profile.DisplayName != request.DisplayName)
        {
            var existingRequest = await changeRequestRepository.GetPendingByProfileAndFieldAsync(
                profile.Id, nameof(ProfileStateDto.DisplayName), cancellationToken);

            if (existingRequest == null)
            {
                changeRequests.Add(ProfileChangeRequest.Create(
                    profile.Id,
                    userId,
                    familyId,
                    nameof(ProfileStateDto.DisplayName),
                    profile.DisplayName.Value,
                    request.DisplayName.Value));
            }
        }

        // Birthday change
        if (request.Birthday.HasValue && profile.Birthday != request.Birthday)
        {
            var existingRequest = await changeRequestRepository.GetPendingByProfileAndFieldAsync(
                profile.Id, nameof(ProfileStateDto.Birthday), cancellationToken);

            if (existingRequest == null)
            {
                changeRequests.Add(ProfileChangeRequest.Create(
                    profile.Id,
                    userId,
                    familyId,
                    nameof(ProfileStateDto.Birthday),
                    profile.Birthday?.Value.ToString("yyyy-MM-dd"),
                    request.Birthday.Value.Value.ToString("yyyy-MM-dd")));
            }
        }

        // Pronouns change
        if (request.Pronouns.HasValue && profile.Pronouns != request.Pronouns)
        {
            var existingRequest = await changeRequestRepository.GetPendingByProfileAndFieldAsync(
                profile.Id, nameof(ProfileStateDto.Pronouns), cancellationToken);

            if (existingRequest == null)
            {
                changeRequests.Add(ProfileChangeRequest.Create(
                    profile.Id,
                    userId,
                    familyId,
                    nameof(ProfileStateDto.Pronouns),
                    profile.Pronouns?.Value,
                    request.Pronouns.Value.Value));
            }
        }

        // Preferences change (stored as JSON)
        if (request.Preferences != null)
        {
            var oldJson = SerializePreferences(profile.Preferences);
            var newJson = SerializePreferences(request.Preferences);

            if (oldJson != newJson)
            {
                var existingRequest = await changeRequestRepository.GetPendingByProfileAndFieldAsync(
                    profile.Id, "Preferences", cancellationToken);

                if (existingRequest == null)
                {
                    changeRequests.Add(ProfileChangeRequest.Create(
                        profile.Id,
                        userId,
                        familyId,
                        "Preferences",
                        oldJson,
                        newJson!));
                }
            }
        }

        // FieldVisibility change (stored as JSON)
        if (request.FieldVisibility != null)
        {
            var oldJson = SerializeFieldVisibility(profile.FieldVisibility);
            var newJson = SerializeFieldVisibility(request.FieldVisibility);

            if (oldJson != newJson)
            {
                var existingRequest = await changeRequestRepository.GetPendingByProfileAndFieldAsync(
                    profile.Id, "FieldVisibility", cancellationToken);

                if (existingRequest == null)
                {
                    changeRequests.Add(ProfileChangeRequest.Create(
                        profile.Id,
                        userId,
                        familyId,
                        "FieldVisibility",
                        oldJson,
                        newJson!));
                }
            }
        }

        // Add all change requests to the repository
        foreach (var changeRequest in changeRequests)
        {
            await changeRequestRepository.AddAsync(changeRequest, cancellationToken);
        }

        // Persist changes
        await dbContext.SaveChangesAsync(cancellationToken);

        LogChangeRequestsCreated(changeRequests.Count, profile.Id.Value);

        return DomainResult.Success(new UpdateUserProfileResult
        {
            ProfileId = profile.Id,
            DisplayName = profile.DisplayName,
            UpdatedAt = profile.UpdatedAt,
            IsNewProfile = false,
            RequiresApproval = changeRequests.Count > 0,
            PendingChangesCount = changeRequests.Count
        });
    }
}
