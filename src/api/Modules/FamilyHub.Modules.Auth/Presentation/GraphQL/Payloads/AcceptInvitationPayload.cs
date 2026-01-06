using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for accepting an invitation.
/// Returns the family the user has joined.
/// </summary>
public sealed record AcceptInvitationPayload : PayloadBase
{
    /// <summary>
    /// The family the user has joined (null if errors occurred).
    /// HotChocolate will automatically map this to FamilyType in the GraphQL schema.
    /// </summary>
    public Family? Family { get; init; }

    /// <summary>
    /// The user's role in the family.
    /// </summary>
    public UserRoleType? Role { get; init; }

    /// <summary>
    /// Constructor for successful payload (called by factory).
    /// </summary>
    /// <param name="family">The family the user joined</param>
    /// <param name="role">The user's role in the family</param>
    public AcceptInvitationPayload(Family family, UserRoleType role)
    {
        Family = family;
        Role = role;
    }

    /// <summary>
    /// Constructor for error payload (called by factory).
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    public AcceptInvitationPayload(IReadOnlyList<UserError> errors) : base(errors)
    {
        Family = null;
        Role = null;
    }
}
