namespace FamilyHub.Modules.UserProfile.Domain.ValueObjects;

/// <summary>
/// Represents user profile preferences for localization and display.
/// This is an embedded value object stored as columns in the profiles table.
/// </summary>
public sealed class ProfilePreferences
{
    /// <summary>
    /// Default language code.
    /// </summary>
    public const string DefaultLanguage = "en";

    /// <summary>
    /// Default timezone identifier.
    /// </summary>
    public const string DefaultTimezone = "UTC";

    /// <summary>
    /// Default date format pattern.
    /// </summary>
    public const string DefaultDateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Maximum length for language code.
    /// </summary>
    public const int LanguageMaxLength = 10;

    /// <summary>
    /// Maximum length for timezone identifier.
    /// </summary>
    public const int TimezoneMaxLength = 50;

    /// <summary>
    /// Maximum length for date format pattern.
    /// </summary>
    public const int DateFormatMaxLength = 20;

    /// <summary>
    /// User's preferred language code (e.g., "en", "de", "es").
    /// </summary>
    public string Language { get; private set; } = DefaultLanguage;

    /// <summary>
    /// User's preferred timezone identifier (e.g., "UTC", "America/New_York").
    /// </summary>
    public string Timezone { get; private set; } = DefaultTimezone;

    /// <summary>
    /// User's preferred date format pattern (e.g., "yyyy-MM-dd", "MM/dd/yyyy").
    /// </summary>
    public string DateFormat { get; private set; } = DefaultDateFormat;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private ProfilePreferences()
    {
    }

    /// <summary>
    /// Creates a new ProfilePreferences with default values.
    /// </summary>
    public static ProfilePreferences Default() => new();

    /// <summary>
    /// Creates a new ProfilePreferences with specified values.
    /// </summary>
    public static ProfilePreferences Create(string? language, string? timezone, string? dateFormat)
    {
        return new ProfilePreferences
        {
            Language = string.IsNullOrWhiteSpace(language) ? DefaultLanguage : language.Trim(),
            Timezone = string.IsNullOrWhiteSpace(timezone) ? DefaultTimezone : timezone.Trim(),
            DateFormat = string.IsNullOrWhiteSpace(dateFormat) ? DefaultDateFormat : dateFormat.Trim()
        };
    }

    /// <summary>
    /// Creates a copy with a different language.
    /// </summary>
    public ProfilePreferences WithLanguage(string language) =>
        Create(language, Timezone, DateFormat);

    /// <summary>
    /// Creates a copy with a different timezone.
    /// </summary>
    public ProfilePreferences WithTimezone(string timezone) =>
        Create(Language, timezone, DateFormat);

    /// <summary>
    /// Creates a copy with a different date format.
    /// </summary>
    public ProfilePreferences WithDateFormat(string dateFormat) =>
        Create(Language, Timezone, dateFormat);
}
