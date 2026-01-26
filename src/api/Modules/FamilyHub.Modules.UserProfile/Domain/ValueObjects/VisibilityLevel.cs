using FamilyHub.Modules.UserProfile.Domain.Constants;

namespace FamilyHub.Modules.UserProfile.Domain.ValueObjects;

/// <summary>
/// Represents a visibility level for profile fields.
/// Controls who can see specific profile information.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct VisibilityLevel
{
    // IMPORTANT: ValidLevels must be declared BEFORE the static readonly fields
    // that use From() to avoid static initialization order issues.
    private static readonly string[] ValidLevels =
    [
        VisibilityLevelConstants.HiddenValue,
        VisibilityLevelConstants.FamilyValue,
        VisibilityLevelConstants.PublicValue
    ];

    /// <summary>
    /// Hidden - field is not visible to anyone except the profile owner.
    /// </summary>
    public static readonly VisibilityLevel Hidden = From(VisibilityLevelConstants.HiddenValue);

    /// <summary>
    /// Family - field is visible to family members only.
    /// </summary>
    public static readonly VisibilityLevel Family = From(VisibilityLevelConstants.FamilyValue);

    /// <summary>
    /// Public - field is visible to all authenticated users.
    /// </summary>
    public static readonly VisibilityLevel Public = From(VisibilityLevelConstants.PublicValue);

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Visibility level cannot be empty.");
        }

        if (!ValidLevels.Contains(value.ToLowerInvariant()))
        {
            return Validation.Invalid($"Invalid visibility level. Must be one of: {string.Join(", ", ValidLevels)}");
        }

        return Validation.Ok;
    }

    /// <summary>
    /// Normalizes input by trimming and converting to lowercase.
    /// </summary>
    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();
}
