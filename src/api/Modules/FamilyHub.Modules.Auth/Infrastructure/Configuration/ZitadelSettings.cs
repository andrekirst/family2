namespace FamilyHub.Modules.Auth.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Zitadel OAuth 2.0/OIDC integration.
/// </summary>
public sealed class ZitadelSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Zitadel";

    /// <summary>
    /// Zitadel instance base URL (e.g., http://localhost:8080 or https://your-instance.zitadel.cloud)
    /// </summary>
    public string Authority { get; init; } = string.Empty;

    /// <summary>
    /// OAuth 2.0 Client ID from Zitadel application configuration
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// OAuth 2.0 Client Secret from Zitadel application configuration
    /// </summary>
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>
    /// Redirect URI where Zitadel will send the authorization code (e.g., http://localhost:4200/auth/callback)
    /// Must match exactly what's configured in Zitadel application
    /// </summary>
    public string RedirectUri { get; init; } = string.Empty;

    /// <summary>
    /// OAuth 2.0 scopes to request (space-separated)
    /// </summary>
    public string Scopes { get; init; } = "openid profile email";

    /// <summary>
    /// Expected audience claim in JWT tokens
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Service Account ID for Zitadel Management API authentication (JWT bearer grant)
    /// Format: {userId}@{projectId} (e.g., 123456789@family_hub)
    /// </summary>
    public string ServiceAccountId { get; init; } = string.Empty;

    /// <summary>
    /// Path to the private key file (PEM format) for signing JWT assertions
    /// Used for service account authentication with Zitadel Management API
    /// </summary>
    public string PrivateKeyPath { get; init; } = string.Empty;

    /// <summary>
    /// Service Account Key ID for JWT header (kid claim)
    /// Used for key identification in JWT assertions
    /// </summary>
    public string ServiceAccountKeyId { get; init; } = string.Empty;

    /// <summary>
    /// OIDC discovery endpoint URL (auto-generated from Authority)
    /// </summary>
    public string MetadataAddress => $"{Authority}/.well-known/openid-configuration";

    /// <summary>
    /// OAuth 2.0 authorization endpoint (auto-generated from Authority)
    /// </summary>
    public string AuthorizationEndpoint => $"{Authority}/oauth/v2/authorize";

    /// <summary>
    /// OAuth 2.0 token endpoint (auto-generated from Authority)
    /// </summary>
    public string TokenEndpoint => $"{Authority}/oauth/v2/token";

    /// <summary>
    /// Validates that all required settings are configured
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Authority)
            && !string.IsNullOrWhiteSpace(ClientId)
            && !string.IsNullOrWhiteSpace(RedirectUri)
            && !string.IsNullOrWhiteSpace(Audience)
            && !string.IsNullOrWhiteSpace(ServiceAccountId)
            && !string.IsNullOrWhiteSpace(PrivateKeyPath)
            && !string.IsNullOrWhiteSpace(ServiceAccountKeyId)
            && File.Exists(PrivateKeyPath);
    }
}
