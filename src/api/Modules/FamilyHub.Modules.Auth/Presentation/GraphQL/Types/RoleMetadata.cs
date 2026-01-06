namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// Rich metadata for a user role.
/// Provides value, label, and description for frontend display.
/// </summary>
public sealed record RoleMetadata
{
    /// <summary>
    /// GraphQL enum value (OWNER, ADMIN, MEMBER, MANAGED_ACCOUNT).
    /// </summary>
    public required UserRoleType Value { get; init; }

    /// <summary>
    /// Human-readable label for UI display.
    /// Example: "Admin", "Member"
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Description of role permissions and responsibilities.
    /// Example: "Can manage family settings and invite members"
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Tailwind CSS class for badge styling.
    /// Example: "bg-purple-100 text-purple-800"
    /// </summary>
    public string? BadgeColorClass { get; init; }
}
