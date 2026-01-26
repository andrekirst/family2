using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetMyProfile;

/// <summary>
/// Result of the GetMyProfile query.
/// Contains the full profile with all fields visible to the owner.
/// </summary>
public sealed record GetMyProfileResult
{
    /// <summary>
    /// The profile ID.
    /// </summary>
    public required UserProfileId ProfileId { get; init; }

    /// <summary>
    /// The user ID that owns this profile.
    /// </summary>
    public required UserId UserId { get; init; }

    /// <summary>
    /// The user's display name.
    /// </summary>
    public required DisplayName DisplayName { get; init; }

    /// <summary>
    /// The user's birthday (if set).
    /// </summary>
    public Birthday? Birthday { get; init; }

    /// <summary>
    /// Calculated age based on birthday (if set).
    /// </summary>
    public int? Age => Birthday?.CalculateAge();

    /// <summary>
    /// The user's preferred pronouns (if set).
    /// </summary>
    public Pronouns? Pronouns { get; init; }

    /// <summary>
    /// The user's localization preferences.
    /// </summary>
    public required ProfilePreferences Preferences { get; init; }

    /// <summary>
    /// The user's field visibility settings.
    /// </summary>
    public required ProfileFieldVisibility FieldVisibility { get; init; }

    /// <summary>
    /// When the profile was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the profile was last updated.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }
}
