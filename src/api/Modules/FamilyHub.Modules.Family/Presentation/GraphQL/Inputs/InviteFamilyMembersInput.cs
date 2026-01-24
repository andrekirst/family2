namespace FamilyHub.Modules.Family.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for batch inviting family members via email.
/// Supports partial success - valid invitations succeed, invalid ones return errors.
/// </summary>
public sealed record InviteFamilyMembersInput
{
    /// <summary>
    /// ID of the family to invite members to.
    /// </summary>
    public required Guid FamilyId { get; init; }

    /// <summary>
    /// List of invitations to process (max 20).
    /// </summary>
    public required IReadOnlyList<InvitationRequestInput> Invitations { get; init; }

    /// <summary>
    /// Optional personal message to include with all invitations.
    /// Maximum 1000 characters.
    /// </summary>
    public string? Message { get; init; }
}

/// <summary>
/// Input for a single invitation within a batch operation.
/// </summary>
public sealed record InvitationRequestInput
{
    /// <summary>
    /// Email address of the invitee.
    /// Must be a valid email format and not already a family member.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Role to assign when invitation is accepted.
    /// Valid values: ADMIN, MEMBER, CHILD (not OWNER).
    /// </summary>
    public required string Role { get; init; }
}
