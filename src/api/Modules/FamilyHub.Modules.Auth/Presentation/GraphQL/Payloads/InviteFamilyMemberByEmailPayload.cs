using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for email-based family member invitation.
/// </summary>
public sealed record InviteFamilyMemberByEmailPayload : PayloadBase
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
    public InviteFamilyMemberByEmailPayload(IReadOnlyList<UserError> errors) : base(errors)
    {
        Invitation = null;
    }
}
