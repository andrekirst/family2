namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for password reset result.
/// </summary>
public sealed class ResetPasswordPayload
{
    /// <summary>
    /// Indicates the password was reset successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Errors that occurred during password reset.
    /// </summary>
    public IReadOnlyList<PayloadError>? Errors { get; init; }
}
