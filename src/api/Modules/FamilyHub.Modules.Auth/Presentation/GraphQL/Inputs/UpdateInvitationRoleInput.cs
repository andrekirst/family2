namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for updating the role of a pending invitation.
/// Allows changing the role before the invitation is accepted.
/// </summary>
public sealed record UpdateInvitationRoleInput
{
    /// <summary>
    /// ID of the invitation to update.
    /// Invitation must be in PENDING status.
    /// </summary>
    public required Guid InvitationId { get; init; }

    /// <summary>
    /// New role to assign when invitation is accepted.
    /// Cannot be OWNER.
    /// </summary>
    public required string NewRole { get; init; }
}
