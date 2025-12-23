namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type representing successful authentication.
/// </summary>
public sealed record AuthenticationResult
{
    /// <summary>
    /// The authenticated user information.
    /// </summary>
    public required UserType User { get; init; }

    /// <summary>
    /// JWT access token for API authentication.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Refresh token for obtaining new access tokens (null for OAuth providers like Zitadel).
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// When the access token expires (UTC).
    /// </summary>
    public required DateTime ExpiresAt { get; init; }
}
