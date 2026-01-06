using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for updating an invitation's role.
/// </summary>
public sealed record UpdateInvitationRolePayload : PayloadBase
{
    /// <summary>
    /// The updated invitation with new role (null if errors occurred).
    /// </summary>
    public PendingInvitationType? Invitation { get; init; }

    /// <summary>
    /// Constructor for successful payload (called by factory).
    /// </summary>
    /// <param name="invitation">The updated invitation</param>
    public UpdateInvitationRolePayload(PendingInvitationType invitation)
    {
        Invitation = invitation;
    }

    /// <summary>
    /// Constructor for error payload (called by factory).
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    public UpdateInvitationRolePayload(IReadOnlyList<UserError> errors) : base(errors)
    {
        Invitation = null;
    }
}
