namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL enum type for FamilyRole.
/// Represents a user's role within a family.
/// </summary>
public enum UserRoleType
{
    /// <summary>
    /// Owner role - full family administration permissions.
    /// Can transfer ownership, remove members, delete family.
    /// </summary>
    OWNER,

    /// <summary>
    /// Admin role - family management permissions.
    /// Can manage members, but cannot transfer ownership.
    /// </summary>
    ADMIN,

    /// <summary>
    /// Member role - standard family member permissions.
    /// Can view family data, manage personal tasks and calendars.
    /// </summary>
    MEMBER,

    /// <summary>
    /// Child role - limited family member permissions.
    /// Age-appropriate access to family features with parental oversight.
    /// </summary>
    CHILD
}
