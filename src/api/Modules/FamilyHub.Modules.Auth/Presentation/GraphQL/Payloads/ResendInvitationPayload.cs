using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for resending an invitation.
/// Generates a new token and extends expiration by 14 days.
/// </summary>
public sealed record ResendInvitationPayload : PayloadBase
{
    /// <summary>
    /// The updated invitation with new token and expiration (null if errors occurred).
    /// </summary>
    public PendingInvitationType? Invitation { get; init; }

    /// <summary>
    /// Constructor for successful payload (called by factory).
    /// </summary>
    /// <param name="invitation">The updated invitation</param>
    public ResendInvitationPayload(PendingInvitationType invitation)
    {
        Invitation = invitation;
    }

    /// <summary>
    /// Constructor for error payload (called by factory).
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    public ResendInvitationPayload(IReadOnlyList<UserError> errors) : base(errors)
    {
        Invitation = null;
    }
}
