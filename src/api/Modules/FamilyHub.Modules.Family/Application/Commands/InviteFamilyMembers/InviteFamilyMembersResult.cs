using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Application.Commands.InviteFamilyMembers;

/// <summary>
/// Result of a batch invitation operation.
/// Supports partial success - valid invitations succeed while invalid ones return errors.
/// </summary>
public record InviteFamilyMembersResult
{
    /// <summary>
    /// Gets the list of successfully created invitations.
    /// </summary>
    public required IReadOnlyList<InvitationSuccess> SuccessfulInvitations { get; init; }

    /// <summary>
    /// Gets the list of failed invitation attempts with error details.
    /// </summary>
    public required IReadOnlyList<InvitationFailure> FailedInvitations { get; init; }

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
/// Represents a successfully created invitation in a batch operation.
/// </summary>
public record InvitationSuccess
{
    /// <summary>
    /// Gets the unique identifier for the invitation.
    /// </summary>
    public required InvitationId InvitationId { get; init; }

    /// <summary>
    /// Gets the email address of the invited user.
    /// </summary>
    public required Email Email { get; init; }

    /// <summary>
    /// Gets the role assigned to the invited family member.
    /// </summary>
    public required FamilyRole Role { get; init; }

    /// <summary>
    /// Gets the invitation token used to accept the invitation.
    /// </summary>
    public required InvitationToken Token { get; init; }

    /// <summary>
    /// Gets the human-readable display code for the invitation.
    /// </summary>
    public required InvitationDisplayCode DisplayCode { get; init; }

    /// <summary>
    /// Gets the expiration date and time of the invitation.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Gets the current status of the invitation.
    /// </summary>
    public required InvitationStatus Status { get; init; }
}

/// <summary>
/// Represents a failed invitation attempt in a batch operation.
/// </summary>
public record InvitationFailure
{
    /// <summary>
    /// Gets the email address that failed to be invited.
    /// </summary>
    public required Email Email { get; init; }

    /// <summary>
    /// Gets the role that was requested for the invitation.
    /// </summary>
    public required FamilyRole Role { get; init; }

    /// <summary>
    /// Gets the error code identifying the failure reason.
    /// </summary>
    public required InvitationErrorCode ErrorCode { get; init; }

    /// <summary>
    /// Gets a human-readable error message describing the failure.
    /// </summary>
    public required string ErrorMessage { get; init; }
}

/// <summary>
/// Error codes for invitation failures in batch operations.
/// </summary>
public enum InvitationErrorCode
{
    /// <summary>
    /// The email is already a member of this family.
    /// </summary>
    ALREADY_MEMBER = 1,

    /// <summary>
    /// The email already has a pending invitation to this family.
    /// </summary>
    DUPLICATE_PENDING_INVITATION = 2,

    /// <summary>
    /// The email is already a member of another family.
    /// </summary>
    MEMBER_OF_ANOTHER_FAMILY = 3,

    /// <summary>
    /// Cannot invite yourself.
    /// </summary>
    SELF_INVITE = 4,

    /// <summary>
    /// Cannot assign the Owner role via invitation.
    /// </summary>
    INVALID_ROLE = 5,

    /// <summary>
    /// Duplicate email in the same batch request.
    /// </summary>
    DUPLICATE_IN_BATCH = 6,

    /// <summary>
    /// Invalid email format.
    /// </summary>
    INVALID_EMAIL = 7,

    /// <summary>
    /// An unexpected error occurred.
    /// </summary>
    UNKNOWN = 99
}
