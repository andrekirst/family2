using FamilyHub.Infrastructure.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type representing a family member with their role.
/// Combines User information with their role in the family context.
/// </summary>
public sealed record FamilyMemberType
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Whether the user's email has been verified.
    /// </summary>
    public required bool EmailVerified { get; init; }

    /// <summary>
    /// The user's role within this family.
    /// </summary>
    public required UserRoleType Role { get; init; }

    /// <summary>
    /// When the user joined the family.
    /// </summary>
    public required DateTime JoinedAt { get; init; }

    /// <summary>
    /// Whether this is the family owner.
    /// Convenience field: true when Role == OWNER.
    /// </summary>
    public required bool IsOwner { get; init; }

    /// <summary>
    /// Audit metadata (creation and last update timestamps).
    /// </summary>
    public required AuditInfoType AuditInfo { get; init; }
}
