namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL subscription payload for pending invitations changes.
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
    public PendingInvitationType? Invitation { get; init; }
}
