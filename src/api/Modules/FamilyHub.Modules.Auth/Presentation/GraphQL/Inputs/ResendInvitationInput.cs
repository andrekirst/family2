namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for resending a pending or expired invitation.
/// Generates a new token and extends expiration by 14 days.
/// </summary>
public sealed record ResendInvitationInput
{
    /// <summary>
    /// ID of the invitation to resend.
    /// Invitation must be in PENDING or EXPIRED status.
    /// </summary>
    public required Guid InvitationId { get; init; }

    /// <summary>
    /// Optional updated message for the resent invitation.
    /// Maximum 500 characters.
    /// </summary>
    public string? Message { get; init; }
}
