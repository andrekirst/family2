namespace FamilyHub.Modules.UserProfile.Domain.Constants;

/// <summary>
/// Constants for visibility level values.
/// Used by VisibilityLevel value object to ensure consistent values across the application.
/// </summary>
public static class VisibilityLevelConstants
{
    /// <summary>
    /// Hidden - field is not visible to anyone except the profile owner.
    /// </summary>
    public const string HiddenValue = "hidden";

    /// <summary>
    /// Family - field is visible to family members only.
    /// </summary>
    public const string FamilyValue = "family";

    /// <summary>
    /// Public - field is visible to all authenticated users.
    /// </summary>
    public const string PublicValue = "public";
}
