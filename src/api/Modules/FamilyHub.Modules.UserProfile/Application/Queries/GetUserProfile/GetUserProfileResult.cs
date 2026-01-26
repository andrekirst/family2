using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetUserProfile;

/// <summary>
/// Result of the GetUserProfile query.
/// Contains profile fields filtered by visibility settings.
/// </summary>
public sealed record GetUserProfileResult
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
    /// The user's display name (always visible).
    /// </summary>
    public required DisplayName DisplayName { get; init; }

    /// <summary>
    /// The user's birthday (null if not visible to requester).
    /// </summary>
    public Birthday? Birthday { get; init; }

    /// <summary>
    /// Calculated age based on birthday (null if birthday not visible).
    /// </summary>
    public int? Age => Birthday?.CalculateAge();

    /// <summary>
    /// The user's preferred pronouns (null if not visible to requester).
    /// </summary>
    public Pronouns? Pronouns { get; init; }

    /// <summary>
    /// The user's localization preferences (null if not visible to requester).
    /// </summary>
    public ProfilePreferences? Preferences { get; init; }

    /// <summary>
    /// When the profile was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
