using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for canceling an invitation.
/// </summary>
[Obsolete("Replaced by Hot Chocolate v14 Mutation Conventions. Remove after frontend migration.")]
public sealed record CancelInvitationPayload
{
    /// <summary>
    /// Indicates whether the cancellation was successful.
    /// True if the invitation was canceled, false if errors occurred.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Constructor for successful payload (called by factory).
    /// </summary>
    public CancelInvitationPayload()
    {
        IsSuccess = true;
    }

    /// <summary>
    /// Constructor for error payload (called by factory).
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    public CancelInvitationPayload(IReadOnlyList<UserError> errors)
    {
        IsSuccess = false;
        Errors = errors;
    }

    /// <summary>
    /// List of errors that occurred during mutation execution.
    /// Null or empty when the mutation succeeded.
    /// </summary>
    public IReadOnlyList<UserError>? Errors { get; init; }
}
