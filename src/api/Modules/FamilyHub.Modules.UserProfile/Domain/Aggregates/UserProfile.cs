using FamilyHub.Modules.UserProfile.Domain.Events;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Aggregates;

/// <summary>
/// UserProfile aggregate root representing a user's profile information.
/// Each user has exactly one profile that stores personal information and preferences.
/// </summary>
public class UserProfile : AggregateRoot<UserProfileId>
{
    /// <summary>
    /// The user ID that owns this profile.
    /// References auth.users but no FK constraint (cross-module).
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// User's display name within the application.
    /// </summary>
    public DisplayName DisplayName { get; private set; }

    /// <summary>
    /// User's birthday (optional).
    /// </summary>
    public Birthday? Birthday { get; private set; }

    /// <summary>
    /// User's preferred pronouns (optional).
    /// </summary>
    public Pronouns? Pronouns { get; private set; }

    /// <summary>
    /// User's localization and display preferences.
    /// </summary>
    public ProfilePreferences Preferences { get; private set; } = ProfilePreferences.Default();

    /// <summary>
    /// Visibility settings for profile fields.
    /// </summary>
    public ProfileFieldVisibility FieldVisibility { get; private set; } = ProfileFieldVisibility.Default();

    /// <summary>
    /// Private constructor for EF Core materialization.
    /// </summary>
    private UserProfile() : base(UserProfileId.From(Guid.Empty))
    {
        UserId = UserId.From(Guid.Empty);
        DisplayName = DisplayName.From("Placeholder");
    }

    private UserProfile(UserProfileId id, UserId userId, DisplayName displayName) : base(id)
    {
        UserId = userId;
        DisplayName = displayName;
        Preferences = ProfilePreferences.Default();
        FieldVisibility = ProfileFieldVisibility.Default();
    }

    /// <summary>
    /// Creates a new user profile.
    /// </summary>
    /// <param name="userId">The user ID that owns this profile.</param>
    /// <param name="displayName">The display name for the profile.</param>
    /// <returns>A new UserProfile instance.</returns>
    public static UserProfile Create(UserId userId, DisplayName displayName)
    {
        var profile = new UserProfile(UserProfileId.New(), userId, displayName);

        profile.AddDomainEvent(new UserProfileCreatedEvent(
            eventVersion: 1,
            profileId: profile.Id,
            userId: userId,
            displayName: displayName));

        return profile;
    }

    /// <summary>
    /// Updates the display name.
    /// </summary>
    public void UpdateDisplayName(DisplayName displayName)
    {
        if (DisplayName == displayName)
        {
            return;
        }

        DisplayName = displayName;
        AddDomainEvent(new UserProfileUpdatedEvent(
            eventVersion: 1,
            profileId: Id,
            updatedField: nameof(DisplayName)));
    }

    /// <summary>
    /// Updates the birthday.
    /// </summary>
    public void UpdateBirthday(Birthday? birthday)
    {
        if (Birthday.Equals(birthday))
        {
            return;
        }

        Birthday = birthday;
        AddDomainEvent(new UserProfileUpdatedEvent(
            eventVersion: 1,
            profileId: Id,
            updatedField: nameof(Birthday)));
    }

    /// <summary>
    /// Updates the pronouns.
    /// </summary>
    public void UpdatePronouns(Pronouns? pronouns)
    {
        if (Pronouns.Equals(pronouns))
        {
            return;
        }

        Pronouns = pronouns;
        AddDomainEvent(new UserProfileUpdatedEvent(
            eventVersion: 1,
            profileId: Id,
            updatedField: nameof(Pronouns)));
    }

    /// <summary>
    /// Updates the profile preferences.
    /// </summary>
    public void UpdatePreferences(ProfilePreferences preferences)
    {
        Preferences = preferences ?? ProfilePreferences.Default();
        AddDomainEvent(new UserProfileUpdatedEvent(
            eventVersion: 1,
            profileId: Id,
            updatedField: nameof(Preferences)));
    }

    /// <summary>
    /// Updates the field visibility settings.
    /// </summary>
    public void UpdateFieldVisibility(ProfileFieldVisibility fieldVisibility)
    {
        FieldVisibility = fieldVisibility ?? ProfileFieldVisibility.Default();
        AddDomainEvent(new UserProfileUpdatedEvent(
            eventVersion: 1,
            profileId: Id,
            updatedField: nameof(FieldVisibility)));
    }

    /// <summary>
    /// Checks if a field is visible to the requesting user based on visibility settings.
    /// </summary>
    /// <param name="fieldName">The name of the field to check.</param>
    /// <param name="requestingUserId">The ID of the user requesting access.</param>
    /// <param name="isSameFamily">Whether the requesting user is in the same family.</param>
    /// <returns>True if the field is visible to the requesting user.</returns>
    public bool IsFieldVisibleTo(string fieldName, UserId requestingUserId, bool isSameFamily)
    {
        // Profile owner always sees everything
        if (UserId == requestingUserId)
        {
            return true;
        }

        var visibility = fieldName switch
        {
            nameof(Birthday) => FieldVisibility.BirthdayVisibility,
            nameof(Pronouns) => FieldVisibility.PronounsVisibility,
            nameof(Preferences) => FieldVisibility.PreferencesVisibility,
            _ => VisibilityLevel.Hidden
        };

        return visibility == VisibilityLevel.Public ||
               (visibility == VisibilityLevel.Family && isSameFamily);
    }

    /// <summary>
    /// Calculates the user's age based on their birthday.
    /// Returns null if birthday is not set.
    /// </summary>
    public int? CalculateAge()
    {
        return Birthday?.CalculateAge();
    }
}
