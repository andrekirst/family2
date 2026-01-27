namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for token refresh result.
/// </summary>
public sealed class RefreshTokenPayload
{
    /// <summary>
    /// New JWT access token.
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// New refresh token (old one is revoked).
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Access token expiration time in seconds.
    /// </summary>
    public int? ExpiresIn { get; init; }

    /// <summary>
    /// Errors that occurred during token refresh.
    /// </summary>
    public IReadOnlyList<PayloadError>? Errors { get; init; }
}
