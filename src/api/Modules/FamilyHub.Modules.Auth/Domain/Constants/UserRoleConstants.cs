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
    /// Managed Account role - accounts managed by family admins (children, elderly, etc.).
    /// Limited permissions with delegated management capabilities.
    /// Requires admin approval for sensitive actions.
    /// </summary>
    public const string ManagedAccountValue = "managed_account";
}