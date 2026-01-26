namespace FamilyHub.Modules.UserProfile.Domain.ValueObjects;

/// <summary>
/// Represents visibility settings for profile fields.
/// Controls who can see specific profile information.
/// This is an embedded value object stored as columns in the profiles table.
/// </summary>
public sealed class ProfileFieldVisibility
{
    /// <summary>
    /// Visibility level for the birthday field.
    /// </summary>
    public VisibilityLevel BirthdayVisibility { get; private set; } = VisibilityLevel.Family;

    /// <summary>
    /// Visibility level for the pronouns field.
    /// </summary>
    public VisibilityLevel PronounsVisibility { get; private set; } = VisibilityLevel.Family;

    /// <summary>
    /// Visibility level for the preferences field.
    /// </summary>
    public VisibilityLevel PreferencesVisibility { get; private set; } = VisibilityLevel.Hidden;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private ProfileFieldVisibility()
    {
    }

    /// <summary>
    /// Creates a new ProfileFieldVisibility with default values.
    /// Default: Birthday and Pronouns visible to family, Preferences hidden.
    /// </summary>
    public static ProfileFieldVisibility Default() => new();

    /// <summary>
    /// Creates a new ProfileFieldVisibility with specified values.
    /// </summary>
    public static ProfileFieldVisibility Create(
        VisibilityLevel birthdayVisibility,
        VisibilityLevel pronounsVisibility,
        VisibilityLevel preferencesVisibility)
    {
        return new ProfileFieldVisibility
        {
            BirthdayVisibility = birthdayVisibility,
            PronounsVisibility = pronounsVisibility,
            PreferencesVisibility = preferencesVisibility
        };
    }

    /// <summary>
    /// Creates a copy with a different birthday visibility.
    /// </summary>
    public ProfileFieldVisibility WithBirthdayVisibility(VisibilityLevel visibility) =>
        Create(visibility, PronounsVisibility, PreferencesVisibility);

    /// <summary>
    /// Creates a copy with a different pronouns visibility.
    /// </summary>
    public ProfileFieldVisibility WithPronounsVisibility(VisibilityLevel visibility) =>
        Create(BirthdayVisibility, visibility, PreferencesVisibility);

    /// <summary>
    /// Creates a copy with a different preferences visibility.
    /// </summary>
    public ProfileFieldVisibility WithPreferencesVisibility(VisibilityLevel visibility) =>
        Create(BirthdayVisibility, PronounsVisibility, visibility);

    /// <summary>
    /// Sets all fields to public visibility.
    /// </summary>
    public static ProfileFieldVisibility AllPublic() =>
        Create(VisibilityLevel.Public, VisibilityLevel.Public, VisibilityLevel.Public);

    /// <summary>
    /// Sets all fields to family-only visibility.
    /// </summary>
    public static ProfileFieldVisibility AllFamily() =>
        Create(VisibilityLevel.Family, VisibilityLevel.Family, VisibilityLevel.Family);

    /// <summary>
    /// Sets all fields to hidden visibility.
    /// </summary>
    public static ProfileFieldVisibility AllHidden() =>
        Create(VisibilityLevel.Hidden, VisibilityLevel.Hidden, VisibilityLevel.Hidden);
}
