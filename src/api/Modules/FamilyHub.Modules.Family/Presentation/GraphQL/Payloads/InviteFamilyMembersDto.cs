namespace FamilyHub.Modules.Family.Presentation.GraphQL.Payloads;

/// <summary>
/// DTO for batch invitation operation result.
/// Supports partial success - valid invitations succeed while invalid ones return errors.
/// Uses primitives instead of Vogen value objects for GraphQL compatibility.
/// </summary>
public sealed record InviteFamilyMembersDto
{
    /// <summary>
    /// Gets the list of successfully created invitations.
    /// </summary>
    public required IReadOnlyList<InvitationSuccessDto> SuccessfulInvitations { get; init; }

    /// <summary>
    /// Gets the list of failed invitation attempts with error details.
    /// </summary>
    public required IReadOnlyList<InvitationFailureDto> FailedInvitations { get; init; }

    /// <summary>
    /// Gets the total number of invitations requested.
    /// </summary>
    public int TotalRequested => SuccessfulInvitations.Count + FailedInvitations.Count;

    /// <summary>
    /// Gets a value indicating whether all invitations succeeded.
    /// </summary>
    public bool AllSucceeded => FailedInvitations.Count == 0;

    /// <summary>
    /// Gets a value indicating whether any invitation succeeded.
    /// </summary>
    public bool AnySucceeded => SuccessfulInvitations.Count > 0;
}

/// <summary>
/// DTO representing a successfully created invitation in a batch operation.
/// </summary>
public sealed record InvitationSuccessDto
{
    /// <summary>
    /// Gets the unique identifier for the invitation.
    /// </summary>
    public required Guid InvitationId { get; init; }

    /// <summary>
    /// Gets the email address of the invited user.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the role assigned to the invited family member.
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Gets the invitation token used to accept the invitation.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Gets the human-readable display code for the invitation.
    /// </summary>
    public required string DisplayCode { get; init; }

    /// <summary>
    /// Gets the expiration date and time of the invitation.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Gets the current status of the invitation.
    /// </summary>
    public required string Status { get; init; }
}

/// <summary>
/// DTO representing a failed invitation attempt in a batch operation.
/// </summary>
public sealed record InvitationFailureDto
{
    /// <summary>
    /// Gets the email address that failed to be invited.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the role that was requested for the invitation.
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Gets the error code identifying the failure reason.
    /// </summary>
    public required string ErrorCode { get; init; }

    /// <summary>
    /// Gets a human-readable error message describing the failure.
    /// </summary>
    public required string ErrorMessage { get; init; }
}
