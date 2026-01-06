namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL enum type for invitation status.
/// Tracks the lifecycle of a family invitation.
/// </summary>
public enum InvitationStatusType
{
    /// <summary>
    /// Invitation has been sent but not yet accepted or rejected.
    /// </summary>
    PENDING,

    /// <summary>
    /// Invitation was accepted by the invitee.
    /// </summary>
    ACCEPTED,

    /// <summary>
    /// Invitation was explicitly rejected by the invitee.
    /// </summary>
    REJECTED,

    /// <summary>
    /// Invitation was cancelled by the inviter before acceptance.
    /// </summary>
    CANCELLED,

    /// <summary>
    /// Invitation expired after timeout period (e.g., 7 days).
    /// </summary>
    EXPIRED
}
