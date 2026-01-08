using System.Security.Cryptography;
using System.Text;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.Modules.Auth.Infrastructure.OAuth;
using IdentityModel;
using Microsoft.Extensions.Options;

namespace FamilyHub.Modules.Auth.Application.Queries.GetAuthUrl;

/// <summary>
/// Handler for GetZitadelAuthUrlQuery.
/// Generates OAuth authorization URL with PKCE (Proof Key for Code Exchange) parameters.
/// </summary>
public sealed partial class GetAuthUrlQueryHandler(
    IOptions<ZitadelSettings> settings,
    ILogger<GetAuthUrlQueryHandler> logger)
    : IRequestHandler<GetAuthUrlQuery, GetAuthUrlResult>
{
    private readonly ZitadelSettings _settings = settings.Value;

    public Task<GetAuthUrlResult> Handle(GetAuthUrlQuery request, CancellationToken cancellationToken)
    {
        LogGeneratingOauthAuthorizationUrlWithPkce(logger);

        // 1. Generate PKCE code verifier (256-bit random string, base64url-encoded)
        var codeVerifier = CryptoRandom.CreateUniqueId(); // 32 bytes = 256 bits

        // 2. Generate PKCE code challenge from verifier (SHA256 hash, base64url-encoded)
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        // 3. Generate state parameter for CSRF protection (128-bit random string)
        var state = CryptoRandom.CreateUniqueId(16);

        // 4. Generate nonce for replay attack protection (128-bit random string)
        var nonce = CryptoRandom.CreateUniqueId(16);

        // 5. Build authorization URL with all OAuth parameters using fluent builder
        var authorizationUrl = new AuthorizationUrlBuilder()
            .WithAuthorizationEndpoint(_settings.AuthorizationEndpoint)
            .WithClientId(_settings.ClientId)
            .WithRedirectUri(_settings.RedirectUri)
            .WithScope(_settings.Scopes)
            .WithCodeChallenge(codeChallenge)
            .WithCodeChallengeMethod("S256") // SHA-256 hashing
            .WithState(state)
            .WithNonce(nonce)
            .WithLoginHint(request.LoginHint) // Phase 5: Pre-fill login form with email/username
            .Build();

        LogGeneratedZitadelAuthorizationUrlAuthorityScopesScopes(logger, _settings.Authority, _settings.Scopes);

        return Task.FromResult(new GetAuthUrlResult
        {
            AuthorizationUrl = authorizationUrl,
            CodeVerifier = codeVerifier,
            State = state
        });
    }

    /// <summary>
    /// Generates PKCE code challenge from code verifier using SHA-256.
    /// </summary>
    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var challengeBytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
        return Base64Url.Encode(challengeBytes);
    }

    [LoggerMessage(LogLevel.Information, "Generating OAuth authorization URL with PKCE")]
    static partial void LogGeneratingOauthAuthorizationUrlWithPkce(ILogger<GetAuthUrlQueryHandler> logger);

    [LoggerMessage(LogLevel.Information, "Generated Zitadel authorization URL: {authority}, Scopes: {scopes}")]
    static partial void LogGeneratedZitadelAuthorizationUrlAuthorityScopesScopes(ILogger<GetAuthUrlQueryHandler> logger, string authority, string scopes);
}
