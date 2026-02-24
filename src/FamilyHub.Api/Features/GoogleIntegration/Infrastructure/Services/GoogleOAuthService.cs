using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using FamilyHub.Api.Features.GoogleIntegration.Models;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;

public sealed class GoogleOAuthService : IGoogleOAuthService
{
    private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string RevokeEndpoint = "https://oauth2.googleapis.com/revoke";
    private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

    private readonly GoogleOAuthOptions _options;
    private readonly HttpClient _httpClient;

    public GoogleOAuthService(IOptions<GoogleOAuthOptions> options, HttpClient httpClient)
    {
        _options = options.Value;
        _httpClient = httpClient;
    }

    public (string AuthUrl, string State, string CodeVerifier) BuildConsentUrl()
    {
        var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var codeVerifier = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var codeChallenge = ComputeCodeChallenge(codeVerifier);

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = _options.Scopes,
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256",
            ["access_type"] = "offline",
            ["prompt"] = "consent"
        };

        var queryString = string.Join("&",
            queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return ($"{AuthorizationEndpoint}?{queryString}", state, codeVerifier);
    }

    public async Task<GoogleTokenResponse> ExchangeCodeForTokensAsync(
        string code, string codeVerifier, CancellationToken ct = default)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUri,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = codeVerifier
        });

        var response = await _httpClient.PostAsync(TokenEndpoint, content, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize Google token response");
    }

    public async Task<GoogleTokenResponse> RefreshAccessTokenAsync(
        string refreshToken, CancellationToken ct = default)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["grant_type"] = "refresh_token"
        });

        var response = await _httpClient.PostAsync(TokenEndpoint, content, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize Google token response");
    }

    public async Task RevokeTokenAsync(string token, CancellationToken ct = default)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["token"] = token
        });

        // Google revocation endpoint returns 200 even if token is already revoked
        await _httpClient.PostAsync(RevokeEndpoint, content, ct);
    }

    public async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GoogleUserInfo>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize Google user info");
    }

    private static string ComputeCodeChallenge(string codeVerifier)
    {
        var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
