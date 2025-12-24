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
    /// Whether this membership is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// When the user joined the family.
    /// </summary>
    public required DateTime JoinedAt { get; init; }
}
