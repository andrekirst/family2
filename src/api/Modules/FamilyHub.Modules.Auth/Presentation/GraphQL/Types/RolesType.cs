namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type for role-related reference data.
/// Provides both all available roles and invitable roles.
/// </summary>
public sealed record RolesType
{
    /// <summary>
    /// All available user roles (OWNER, ADMIN, MEMBER).
    /// </summary>
    public required IReadOnlyList<RoleMetadata> All { get; init; }

    /// <summary>
    /// Roles available for invitations (excludes OWNER).
    /// </summary>
    public required IReadOnlyList<RoleMetadata> Invitable { get; init; }
}
