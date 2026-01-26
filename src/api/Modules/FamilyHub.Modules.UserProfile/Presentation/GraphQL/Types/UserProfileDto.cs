namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL output type for a full user profile (owner view).
/// </summary>
public sealed record UserProfileDto
{
    /// <summary>
    /// The profile ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The user ID that owns this profile.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// The user's birthday (if set).
    /// </summary>
    public DateOnly? Birthday { get; init; }

    /// <summary>
    /// Calculated age based on birthday.
    /// </summary>
    public int? Age { get; init; }

    /// <summary>
    /// The user's preferred pronouns (if set).
    /// </summary>
    public string? Pronouns { get; init; }

    /// <summary>
    /// Profile preferences.
    /// </summary>
    public required ProfilePreferencesDto Preferences { get; init; }

    /// <summary>
    /// Field visibility settings.
    /// </summary>
    public required ProfileFieldVisibilityDto FieldVisibility { get; init; }

    /// <summary>
    /// When the profile was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the profile was last updated.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }
}

/// <summary>
/// GraphQL output type for profile preferences.
/// </summary>
public sealed record ProfilePreferencesDto
{
    /// <summary>
    /// Language code.
    /// </summary>
    public required string Language { get; init; }

    /// <summary>
    /// Timezone identifier.
    /// </summary>
    public required string Timezone { get; init; }

    /// <summary>
    /// Date format pattern.
    /// </summary>
    public required string DateFormat { get; init; }
}

/// <summary>
/// GraphQL output type for field visibility settings.
/// </summary>
public sealed record ProfileFieldVisibilityDto
{
    /// <summary>
    /// Visibility level for birthday.
    /// </summary>
    public required string BirthdayVisibility { get; init; }

    /// <summary>
    /// Visibility level for pronouns.
    /// </summary>
    public required string PronounsVisibility { get; init; }

    /// <summary>
    /// Visibility level for preferences.
    /// </summary>
    public required string PreferencesVisibility { get; init; }
}

/// <summary>
/// GraphQL output type for viewing another user's profile (visibility-filtered).
/// </summary>
public sealed record PublicUserProfileDto
{
    /// <summary>
    /// The profile ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The user ID that owns this profile.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's display name (always visible).
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// The user's birthday (null if not visible).
    /// </summary>
    public DateOnly? Birthday { get; init; }

    /// <summary>
    /// Calculated age based on birthday (null if birthday not visible).
    /// </summary>
    public int? Age { get; init; }

    /// <summary>
    /// The user's preferred pronouns (null if not visible).
    /// </summary>
    public string? Pronouns { get; init; }

    /// <summary>
    /// Profile preferences (null if not visible).
    /// </summary>
    public ProfilePreferencesDto? Preferences { get; init; }

    /// <summary>
    /// When the profile was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}

/// <summary>
/// GraphQL output type for update profile result.
/// </summary>
public sealed record UpdateUserProfileDto
{
    /// <summary>
    /// The profile ID.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// The updated display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// When the profile was last updated.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Whether a new profile was created.
    /// </summary>
    public required bool IsNewProfile { get; init; }

    /// <summary>
    /// Whether the update requires parent approval (true for child users).
    /// </summary>
    public bool RequiresApproval { get; init; }

    /// <summary>
    /// Number of pending change requests created (for child users).
    /// </summary>
    public int PendingChangesCount { get; init; }
}
