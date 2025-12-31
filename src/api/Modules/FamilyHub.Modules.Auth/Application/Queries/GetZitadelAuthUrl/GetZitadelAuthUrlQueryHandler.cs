using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.Modules.Auth.Infrastructure.OAuth;
using IdentityModel;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace FamilyHub.Modules.Auth.Application.Queries.GetZitadelAuthUrl;

/// <summary>
/// Handler for GetZitadelAuthUrlQuery.
/// Generates OAuth authorization URL with PKCE (Proof Key for Code Exchange) parameters.
/// </summary>
public sealed partial class GetZitadelAuthUrlQueryHandler(
    IOptions<ZitadelSettings> settings,
    ILogger<GetZitadelAuthUrlQueryHandler> logger)
    : IRequestHandler<GetZitadelAuthUrlQuery, GetZitadelAuthUrlResult>
{
    private readonly ZitadelSettings? _settings = settings.Value;

    public Task<GetZitadelAuthUrlResult> Handle(GetZitadelAuthUrlQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating Zitadel OAuth authorization URL with PKCE");

        // 1. Generate PKCE code verifier (256-bit random string, base64url-encoded)
        var codeVerifier = CryptoRandom.CreateUniqueId(); // 32 bytes = 256 bits

        // 2. Generate PKCE code challenge from verifier (SHA256 hash, base64url-encoded)
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        // 3. Generate state parameter for CSRF protection (128-bit random string)
        var state = CryptoRandom.CreateUniqueId(16);

        // 4. Generate nonce for replay attack protection (128-bit random string)
        var nonce = CryptoRandom.CreateUniqueId(16);

        ArgumentNullException.ThrowIfNull(_settings);
        
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
            .Build();

        LogGeneratedZitadelAuthorizationUrlAuthorityScopesScopes(logger, _settings.Authority, _settings.Scopes);

        return Task.FromResult(new GetZitadelAuthUrlResult
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

    [LoggerMessage(LogLevel.Information, "Generated Zitadel authorization URL: {authority}, Scopes: {scopes}")]
    static partial void LogGeneratedZitadelAuthorizationUrlAuthorityScopesScopes(ILogger<GetZitadelAuthUrlQueryHandler> logger, string authority, string scopes);
}
