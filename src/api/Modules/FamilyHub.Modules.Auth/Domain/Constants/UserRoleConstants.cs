namespace FamilyHub.Modules.Auth.Domain.Constants;

/// <summary>
/// User role constants for the Auth module.
/// Centralizes role names to prevent string typos and ensure consistency.
/// </summary>
public static class UserRoleConstants
{
    /// <summary>
    /// Owner role - full family administration permissions.
    /// Can transfer ownership, remove members, delete family.
    /// </summary>
    public const string OwnerValue = "owner";

    /// <summary>
    /// Admin role - family management permissions.
    /// Can manage members, but cannot transfer ownership.
    /// </summary>
    public const string AdminValue = "admin";

    /// <summary>
    /// Member role - standard family member permissions.
    /// Can view family data, manage personal tasks and calendars.
    /// </summary>
    public const string MemberValue = "member";

    /// <summary>
    /// Child role - limited permissions for child accounts.
    /// Read-only access to family calendar and shared lists.
    /// Requires parental approval for actions.
    /// </summary>
    public const string ChildValue = "child";
}