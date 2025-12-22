namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for Zitadel authorization URL.
/// </summary>
public sealed record GetZitadelAuthUrlPayload
{
    /// <summary>
    /// Complete Zitadel OAuth authorization URL
    /// </summary>
    public required string AuthorizationUrl { get; init; }

    /// <summary>
    /// PKCE code verifier (frontend must store this)
    /// </summary>
    public required string CodeVerifier { get; init; }

    /// <summary>
    /// State parameter for CSRF protection
    /// </summary>
    public required string State { get; init; }
}
