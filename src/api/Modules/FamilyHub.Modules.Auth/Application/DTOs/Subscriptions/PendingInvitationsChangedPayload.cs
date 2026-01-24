namespace FamilyHub.Modules.Auth.Application.DTOs.Subscriptions;

/// <summary>
/// Subscription payload for pending invitations changes.
/// Published when an invitation is created, updated, accepted, or canceled.
/// </summary>
public sealed record PendingInvitationsChangedPayload
{
    /// <summary>
    /// ID of the family where the change occurred.
    /// </summary>
    public required Guid FamilyId { get; init; }

    /// <summary>
    /// Type of change (ADDED, UPDATED, REMOVED).
    /// </summary>
    public required ChangeType ChangeType { get; init; }

    /// <summary>
    /// The invitation that changed.
    /// Null for REMOVED events (invitation accepted/canceled).
    /// </summary>
    public PendingInvitationDto? Invitation { get; init; }
}
