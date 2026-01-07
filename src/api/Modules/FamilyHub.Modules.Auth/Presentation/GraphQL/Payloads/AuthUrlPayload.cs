namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for OAuth authorization URL.
/// Provider-agnostic to support multiple authentication systems (Zitadel, Auth0, etc.).
/// </summary>
public sealed record AuthUrlPayload
{
    /// <summary>
    /// Complete OAuth authorization URL to redirect the user to.
    /// </summary>
    public required string AuthorizationUrl { get; init; }

    /// <summary>
    /// PKCE code verifier that must be stored by the client.
    /// Required for secure authorization code exchange.
    /// </summary>
    public required string CodeVerifier { get; init; }

    /// <summary>
    /// State parameter for CSRF protection.
    /// Must be validated during the OAuth callback.
    /// </summary>
    public required string State { get; init; }
}
