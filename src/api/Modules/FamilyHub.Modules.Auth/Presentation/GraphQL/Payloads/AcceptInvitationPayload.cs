using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for accepting an invitation.
/// Returns the family information and role after successfully joining.
/// </summary>
[Obsolete("Replaced by Hot Chocolate v14 Mutation Conventions. Remove after frontend migration.")]
public sealed record AcceptInvitationPayload
{
    /// <summary>
    /// The family ID the user joined (null if errors occurred).
    /// </summary>
    public Guid? FamilyId { get; init; }

    /// <summary>
    /// The family name.
    /// </summary>
    public string? FamilyName { get; init; }

    /// <summary>
    /// The user's role in the family.
    /// </summary>
    public UserRoleType? Role { get; init; }

    /// <summary>
    /// Constructor for successful payload (uses data from AcceptInvitationResult).
    /// </summary>
    /// <param name="familyId">The family ID the user joined</param>
    /// <param name="familyName">The family name</param>
    /// <param name="role">The user's role in the family</param>
    public AcceptInvitationPayload(Guid familyId, string familyName, UserRoleType role)
    {
        FamilyId = familyId;
        FamilyName = familyName;
        Role = role;
    }

    /// <summary>
    /// Constructor for error payload.
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    public AcceptInvitationPayload(IReadOnlyList<UserError> errors)
    {
        FamilyId = null;
        FamilyName = null;
        Role = null;
        Errors = errors;
    }

    /// <summary>
    /// List of errors that occurred during mutation execution.
    /// Null or empty when the mutation succeeded.
    /// </summary>
    public IReadOnlyList<UserError>? Errors { get; init; }
}
