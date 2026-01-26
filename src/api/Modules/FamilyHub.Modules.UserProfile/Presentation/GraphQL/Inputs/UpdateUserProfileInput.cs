namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for updating a user profile.
/// All primitive types for JSON deserialization.
/// </summary>
public sealed record UpdateUserProfileInput
{
    /// <summary>
    /// The display name for the profile (required).
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// The user's birthday (optional).
    /// </summary>
    public DateOnly? Birthday { get; init; }

    /// <summary>
    /// The user's preferred pronouns (optional).
    /// </summary>
    public string? Pronouns { get; init; }

    /// <summary>
    /// Profile preferences (optional).
    /// </summary>
    public ProfilePreferencesInput? Preferences { get; init; }

    /// <summary>
    /// Field visibility settings (optional).
    /// </summary>
    public ProfileFieldVisibilityInput? FieldVisibility { get; init; }
}

/// <summary>
/// GraphQL input for profile preferences.
/// </summary>
public sealed record ProfilePreferencesInput
{
    /// <summary>
    /// Language code (e.g., "en", "de").
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Timezone identifier (e.g., "UTC", "America/New_York").
    /// </summary>
    public string? Timezone { get; init; }

    /// <summary>
    /// Date format pattern (e.g., "yyyy-MM-dd").
    /// </summary>
    public string? DateFormat { get; init; }
}

/// <summary>
/// GraphQL input for field visibility settings.
/// </summary>
public sealed record ProfileFieldVisibilityInput
{
    /// <summary>
    /// Visibility level for birthday: "hidden", "family", or "public".
    /// </summary>
    public string? BirthdayVisibility { get; init; }

    /// <summary>
    /// Visibility level for pronouns: "hidden", "family", or "public".
    /// </summary>
    public string? PronounsVisibility { get; init; }

    /// <summary>
    /// Visibility level for preferences: "hidden", "family", or "public".
    /// </summary>
    public string? PreferencesVisibility { get; init; }
}
