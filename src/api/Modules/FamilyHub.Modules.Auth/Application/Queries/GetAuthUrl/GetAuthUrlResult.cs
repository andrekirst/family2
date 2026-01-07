namespace FamilyHub.Modules.Auth.Application.Queries.GetAuthUrl;

/// <summary>
/// Result containing the authorization URL and PKCE code verifier.
/// </summary>
public sealed record GetAuthUrlResult
{
    /// <summary>
    /// Complete OAuth authorization URL with PKCE parameters.
    /// Frontend should redirect user to this URL.
    /// </summary>
    public required string AuthorizationUrl { get; init; }

    /// <summary>
    /// PKCE code verifier (256-bit random string).
    /// Frontend must store this securely and send it back during token exchange.
    /// </summary>
    public required string CodeVerifier { get; init; }

    /// <summary>
    /// State parameter for CSRF protection.
    /// Frontend should validate this matches when receiving the callback.
    /// </summary>
    public required string State { get; init; }
}
