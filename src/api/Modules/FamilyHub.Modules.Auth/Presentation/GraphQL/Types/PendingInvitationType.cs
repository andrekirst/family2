namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type representing a family invitation.
/// Used for Phase 1+ invitation system (Epic #24).
/// Email-based invitations only.
/// </summary>
public sealed record PendingInvitationType
{
    /// <summary>
    /// Unique identifier for this invitation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Email address of the invitee.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Role that will be assigned when invitation is accepted.
    /// </summary>
    public required UserRoleType Role { get; init; }

    /// <summary>
    /// Current status of the invitation.
    /// </summary>
    public required InvitationStatusType Status { get; init; }

    /// <summary>
    /// User who sent the invitation.
    /// </summary>
    public Guid? InvitedById { get; init; }

    /// <summary>
    /// When the invitation was sent.
    /// </summary>
    public required DateTime InvitedAt { get; init; }

    /// <summary>
    /// When the invitation will expire (if not accepted).
    /// Typically 14 days for email invitations.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Whether the invitation has expired.
    /// Convenience field: true when DateTime.UtcNow > ExpiresAt.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Optional personal message included with the invitation.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Display code for debugging/support (only visible to admins).
    /// Not included in public InvitationByToken query.
    /// </summary>
    public string? DisplayCode { get; init; }
}
