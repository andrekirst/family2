namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for inviting a family member via email.
/// Sends a token-based invitation with 14-day expiration.
/// </summary>
public sealed record InviteFamilyMemberByEmailInput
{
    /// <summary>
    /// ID of the family to invite the member to.
    /// </summary>
    public required Guid FamilyId { get; init; }

    /// <summary>
    /// Email address of the invitee.
    /// Must be a valid email format and not already a family member.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Role to assign when invitation is accepted.
    /// Cannot be OWNER (ownership must be transferred separately).
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Optional personal message to include with the invitation.
    /// Maximum 500 characters.
    /// </summary>
    public string? Message { get; init; }
}
