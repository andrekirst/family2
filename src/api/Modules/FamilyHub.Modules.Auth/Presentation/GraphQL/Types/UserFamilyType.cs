using FamilyHub.Infrastructure.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type representing a user's membership in a family.
/// </summary>
public sealed record UserFamilyType
{
    /// <summary>
    /// The user's unique identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The family's unique identifier.
    /// </summary>
    public required Guid FamilyId { get; init; }

    /// <summary>
    /// The user's role in this family (owner, admin, member, child).
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Audit metadata (membership creation and last update timestamps).
    /// </summary>
    public required AuditInfoType AuditInfo { get; init; }
}
