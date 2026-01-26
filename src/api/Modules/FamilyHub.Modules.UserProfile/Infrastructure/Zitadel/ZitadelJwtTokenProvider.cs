using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FamilyHub.Modules.UserProfile.Infrastructure.Zitadel;

/// <summary>
/// Provides JWT tokens for Zitadel Management API authentication using service account credentials.
/// Uses JWT bearer grant type (RFC 7523) to exchange a signed JWT assertion for an access token.
/// </summary>
public sealed partial class ZitadelJwtTokenProvider : IZitadelTokenProvider
{
    private readonly ZitadelSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ZitadelJwtTokenProvider> _logger;

    private const string CacheKey = "ZitadelManagementApiToken";
    private static readonly TimeSpan TokenExpiryBuffer = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="ZitadelJwtTokenProvider"/> class.
    /// </summary>
    public ZitadelJwtTokenProvider(
        IOptions<ZitadelSettings> settings,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<ZitadelJwtTokenProvider> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Try to get cached token
        if (_cache.TryGetValue(CacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            LogUsingCachedToken();
            return cachedToken;
        }

        LogRequestingNewToken();

        // Create JWT assertion
        var jwtAssertion = CreateJwtAssertion();

        // Exchange JWT for access token
        var accessToken = await ExchangeJwtForAccessTokenAsync(jwtAssertion, cancellationToken);

        // Cache the token
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) - TokenExpiryBuffer
        };
        _cache.Set(CacheKey, accessToken, cacheOptions);

        LogTokenObtained();
        return accessToken;
    }

    /// <summary>
    /// Creates a signed JWT assertion for the service account.
    /// </summary>
    private string CreateJwtAssertion()
    {
        var privateKey = LoadPrivateKey();

        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Iss, _settings.ServiceAccountId),
            new(JwtRegisteredClaimNames.Sub, _settings.ServiceAccountId),
            new(JwtRegisteredClaimNames.Aud, _settings.Authority),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Exp, new DateTimeOffset(now.AddHours(1)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var signingCredentials = new SigningCredentials(
            new RsaSecurityKey(privateKey) { KeyId = _settings.ServiceAccountKeyId },
            SecurityAlgorithms.RsaSha256);

        var jwt = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    /// <summary>
    /// Loads the RSA private key from the configured file path.
    /// </summary>
    private RSA LoadPrivateKey()
    {
        if (!File.Exists(_settings.PrivateKeyPath))
        {
            throw new InvalidOperationException($"Private key file not found: {_settings.PrivateKeyPath}");
        }

        var keyContent = File.ReadAllText(_settings.PrivateKeyPath);
        var rsa = RSA.Create();

        // Try PKCS#8 format first (common for Zitadel exported keys)
        try
        {
            rsa.ImportFromPem(keyContent);
            return rsa;
        }
        catch
        {
            // If that fails, try other formats
            rsa.Dispose();
            throw new InvalidOperationException("Failed to load private key. Ensure it's in PEM format (PKCS#8).");
        }
    }

    /// <summary>
    /// Exchanges the JWT assertion for an access token using OAuth 2.0 JWT bearer grant.
    /// </summary>
    private async Task<string> ExchangeJwtForAccessTokenAsync(
        string jwtAssertion,
        CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
            ["assertion"] = jwtAssertion,
            ["scope"] = "openid urn:zitadel:iam:org:project:id:zitadel:aud"
        });

        var response = await httpClient.PostAsync(
            _settings.TokenEndpoint,
            tokenRequest,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            LogTokenExchangeFailed(response.StatusCode.ToString(), errorContent);
            throw new InvalidOperationException($"Failed to exchange JWT for access token: {response.StatusCode} - {errorContent}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        if (tokenResponse?.AccessToken == null)
        {
            throw new InvalidOperationException("Token response did not contain an access token");
        }

        return tokenResponse.AccessToken;
    }

    /// <summary>
    /// Invalidates the cached token, forcing a new token to be obtained on next request.
    /// </summary>
    public void InvalidateToken()
    {
        _cache.Remove(CacheKey);
        LogTokenInvalidated();
    }

    [LoggerMessage(LogLevel.Debug, "Using cached Zitadel Management API token")]
    partial void LogUsingCachedToken();

    [LoggerMessage(LogLevel.Information, "Requesting new Zitadel Management API token")]
    partial void LogRequestingNewToken();

    [LoggerMessage(LogLevel.Information, "Successfully obtained Zitadel Management API token")]
    partial void LogTokenObtained();

    [LoggerMessage(LogLevel.Error, "Failed to exchange JWT for access token: {StatusCode} - {ErrorContent}")]
    partial void LogTokenExchangeFailed(string statusCode, string errorContent);

    [LoggerMessage(LogLevel.Debug, "Invalidated cached Zitadel Management API token")]
    partial void LogTokenInvalidated();

    private sealed record TokenResponse(
        string? AccessToken,
        string? TokenType,
        int? ExpiresIn);
}
