namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for logout result.
/// </summary>
public sealed class LogoutPayload
{
    /// <summary>
    /// Indicates the logout was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Number of sessions (refresh tokens) that were revoked.
    /// </summary>
    public int RevokedSessionCount { get; init; }

    /// <summary>
    /// Errors that occurred during logout.
    /// </summary>
    public IReadOnlyList<PayloadError>? Errors { get; init; }
}
