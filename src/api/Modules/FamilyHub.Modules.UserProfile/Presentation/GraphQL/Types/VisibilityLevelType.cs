namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL enum for visibility levels.
/// Maps to the domain VisibilityLevel value object.
/// </summary>
public enum VisibilityLevelType
{
    /// <summary>
    /// Field is hidden from everyone except the profile owner.
    /// </summary>
    HIDDEN,

    /// <summary>
    /// Field is visible to family members only.
    /// </summary>
    FAMILY,

    /// <summary>
    /// Field is visible to all authenticated users.
    /// </summary>
    PUBLIC
}
