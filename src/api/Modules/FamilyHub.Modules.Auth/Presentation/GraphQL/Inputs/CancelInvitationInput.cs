namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for canceling a pending invitation.
/// Only OWNER or ADMIN can cancel invitations.
/// </summary>
public sealed record CancelInvitationInput
{
    /// <summary>
    /// ID of the invitation to cancel.
    /// Invitation must be in PENDING status.
    /// </summary>
    public required Guid InvitationId { get; init; }
}
