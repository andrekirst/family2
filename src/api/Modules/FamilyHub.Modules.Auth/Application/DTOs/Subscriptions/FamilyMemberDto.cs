namespace FamilyHub.Modules.Auth.Application.DTOs.Subscriptions;

/// <summary>
/// DTO representing a family member for subscription payloads.
/// </summary>
public sealed record FamilyMemberDto
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
    /// The user's role within this family (e.g., "owner", "admin", "member").
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// When the user joined the family.
    /// </summary>
    public required DateTime JoinedAt { get; init; }

    /// <summary>
    /// Whether this is the family owner.
    /// </summary>
    public required bool IsOwner { get; init; }

    /// <summary>
    /// When the record was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
