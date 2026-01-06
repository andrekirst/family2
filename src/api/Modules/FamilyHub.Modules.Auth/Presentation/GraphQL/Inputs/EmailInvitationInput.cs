namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for a single email invitation within a batch.
/// Part of BatchInviteFamilyMembersInput for mixed-mode batch invitations.
/// </summary>
public sealed record EmailInvitationInput
{
    /// <summary>
    /// Email address of the invitee.
    /// Must be a valid email format and not already a family member.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Role to assign when invitation is accepted.
    /// Cannot be OWNER.
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Optional personal message to include with the invitation.
    /// Maximum 500 characters.
    /// </summary>
    public string? Message { get; init; }
}
