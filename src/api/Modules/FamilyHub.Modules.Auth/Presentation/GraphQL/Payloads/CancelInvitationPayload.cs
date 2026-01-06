using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for canceling an invitation.
/// </summary>
public sealed record CancelInvitationPayload : PayloadBase
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
    public CancelInvitationPayload(IReadOnlyList<UserError> errors) : base(errors)
    {
        IsSuccess = false;
    }
}
