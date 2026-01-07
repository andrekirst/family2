namespace FamilyHub.Modules.Auth.Application.Constants;

/// <summary>
/// Constants for authorization policy names used in ASP.NET Core authorization.
/// These policy names map to the custom authorization requirements and handlers.
/// </summary>
public static class AuthorizationPolicyConstants
{
    /// <summary>
    /// Policy name that requires the user to have the Owner role.
    /// </summary>
    public const string RequireOwner = "RequireOwner";

    /// <summary>
    /// Policy name that requires the user to have the Admin role.
    /// </summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>
    /// Policy name that requires the user to have either the Owner or Admin role.
    /// </summary>
    public const string RequireOwnerOrAdmin = "RequireOwnerOrAdmin";
}
