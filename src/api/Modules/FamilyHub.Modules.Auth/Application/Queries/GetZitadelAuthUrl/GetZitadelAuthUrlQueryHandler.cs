using FamilyHub.Modules.Auth.Infrastructure.Configuration;
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
public sealed class GetZitadelAuthUrlQueryHandler(
    IOptions<ZitadelSettings> settings,
    ILogger<GetZitadelAuthUrlQueryHandler> logger)
    : IRequestHandler<GetZitadelAuthUrlQuery, GetZitadelAuthUrlResult>
{
    private readonly ZitadelSettings _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    private readonly ILogger<GetZitadelAuthUrlQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task<GetZitadelAuthUrlResult> Handle(GetZitadelAuthUrlQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating Zitadel OAuth authorization URL with PKCE");

        // 1. Generate PKCE code verifier (256-bit random string, base64url-encoded)
        var codeVerifier = CryptoRandom.CreateUniqueId(); // 32 bytes = 256 bits

        // 2. Generate PKCE code challenge from verifier (SHA256 hash, base64url-encoded)
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        // 3. Generate state parameter for CSRF protection (128-bit random string)
        var state = CryptoRandom.CreateUniqueId(16);

        // 4. Generate nonce for replay attack protection (128-bit random string)
        var nonce = CryptoRandom.CreateUniqueId(16);

        // 5. Build authorization URL with all OAuth parameters
        // TODO Create a AuthorizationUrlBuilder
        var authorizationUrl = $"{_settings.AuthorizationEndpoint}?" +
            $"client_id={Uri.EscapeDataString(_settings.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(_settings.RedirectUri)}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(_settings.Scopes)}" +
            $"&code_challenge={codeChallenge}" +
            $"&code_challenge_method=S256" + // SHA-256 hashing
            $"&state={state}" +
            $"&nonce={nonce}";

        _logger.LogInformation(
            "Generated Zitadel authorization URL: {Authority}, Scopes: {Scopes}",
            _settings.Authority,
            _settings.Scopes);

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
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Base64Url.Encode(challengeBytes);
    }
}
