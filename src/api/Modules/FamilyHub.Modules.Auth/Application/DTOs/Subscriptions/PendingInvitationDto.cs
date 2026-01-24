namespace FamilyHub.Modules.Auth.Application.DTOs.Subscriptions;

/// <summary>
/// DTO representing a pending invitation for subscription payloads.
/// </summary>
public sealed record PendingInvitationDto
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
    /// Role that will be assigned when invitation is accepted (e.g., "admin", "member").
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Current status of the invitation (e.g., "pending", "accepted", "expired").
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// User ID who sent the invitation.
    /// </summary>
    public Guid? InvitedById { get; init; }

    /// <summary>
    /// When the invitation was sent.
    /// </summary>
    public required DateTime InvitedAt { get; init; }

    /// <summary>
    /// When the invitation will expire (if not accepted).
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Optional personal message included with the invitation.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Display code for debugging/support.
    /// </summary>
    public string? DisplayCode { get; init; }
}
