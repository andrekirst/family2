using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for updating an invitation's role.
/// Returns the updated invitation ID and new role.
/// </summary>
[Obsolete("Replaced by Hot Chocolate v14 Mutation Conventions. Remove after frontend migration.")]
public sealed record UpdateInvitationRolePayload
{
    /// <summary>
    /// The invitation ID that was updated (null if errors occurred).
    /// </summary>
    public Guid? InvitationId { get; init; }

    /// <summary>
    /// The new role assigned to the invitation.
    /// </summary>
    public UserRoleType? Role { get; init; }

    /// <summary>
    /// Constructor for successful payload (uses data from UpdateInvitationRoleResult).
    /// </summary>
    /// <param name="invitationId">The invitation ID that was updated</param>
    /// <param name="role">The new role</param>
    public UpdateInvitationRolePayload(Guid invitationId, UserRoleType role)
    {
        InvitationId = invitationId;
        Role = role;
    }

    /// <summary>
    /// Constructor for error payload.
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    public UpdateInvitationRolePayload(IReadOnlyList<UserError> errors)
    {
        InvitationId = null;
        Role = null;
        Errors = errors;
    }

    /// <summary>
    /// List of errors that occurred during mutation execution.
    /// Null or empty when the mutation succeeded.
    /// </summary>
    public IReadOnlyList<UserError>? Errors { get; init; }
}
