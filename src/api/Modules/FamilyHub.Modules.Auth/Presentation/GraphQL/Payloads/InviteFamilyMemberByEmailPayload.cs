using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for email-based family member invitation.
/// </summary>
[Obsolete("Replaced by Hot Chocolate v14 Mutation Conventions. Remove after frontend migration.")]
public sealed record InviteFamilyMemberByEmailPayload
{
    /// <summary>
    /// The created invitation (null if errors occurred).
    /// Contains invitation token, expiration, and display code.
    /// </summary>
    public PendingInvitationType? Invitation { get; init; }

    /// <summary>
    /// Constructor for successful payload (called by factory).
    /// </summary>
    /// <param name="invitation">The created invitation</param>
    public InviteFamilyMemberByEmailPayload(PendingInvitationType invitation)
    {
        Invitation = invitation;
    }

    /// <summary>
    /// Constructor for error payload (called by factory).
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    public InviteFamilyMemberByEmailPayload(IReadOnlyList<UserError> errors)
    {
        Invitation = null;
        Errors = errors;
    }

    /// <summary>
    /// List of errors that occurred during mutation execution.
    /// Null or empty when the mutation succeeded.
    /// </summary>
    public IReadOnlyList<UserError>? Errors { get; init; }
}
