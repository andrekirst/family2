namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for password change result.
/// </summary>
public sealed class ChangePasswordPayload
{
    /// <summary>
    /// Indicates the password was changed successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Errors that occurred during password change.
    /// </summary>
    public IReadOnlyList<PayloadError>? Errors { get; init; }
}
