using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;
using FamilyHub.Api.Features.GoogleIntegration.Models;

namespace FamilyHub.TestCommon.Fakes;

public class FakeGoogleOAuthService : IGoogleOAuthService
{
    public string ConsentUrl { get; set; } = "https://accounts.google.com/o/oauth2/v2/auth?test=true";
    public string GeneratedState { get; set; } = "test-state";
    public string GeneratedCodeVerifier { get; set; } = "test-code-verifier";

    public GoogleTokenResponse TokenResponse { get; set; } = new()
    {
        AccessToken = "fake-access-token",
        RefreshToken = "fake-refresh-token",
        ExpiresIn = 3600,
        TokenType = "Bearer",
        Scope = "openid email profile https://www.googleapis.com/auth/calendar.readonly https://www.googleapis.com/auth/calendar.events"
    };

    public GoogleUserInfo UserInfo { get; set; } = new()
    {
        Sub = "google-user-123",
        Email = "test@gmail.com"
    };

    public bool TokenRevoked { get; private set; }
    public string? RevokedToken { get; private set; }

    public (string AuthUrl, string State, string CodeVerifier) BuildConsentUrl()
        => (ConsentUrl, GeneratedState, GeneratedCodeVerifier);

    public Task<GoogleTokenResponse> ExchangeCodeForTokensAsync(
        string code, string codeVerifier, CancellationToken ct = default)
        => Task.FromResult(TokenResponse);

    public Task<GoogleTokenResponse> RefreshAccessTokenAsync(
        string refreshToken, CancellationToken ct = default)
        => Task.FromResult(TokenResponse);

    public Task RevokeTokenAsync(string token, CancellationToken ct = default)
    {
        TokenRevoked = true;
        RevokedToken = token;
        return Task.CompletedTask;
    }

    public Task<GoogleUserInfo> GetUserInfoAsync(string accessToken, CancellationToken ct = default)
        => Task.FromResult(UserInfo);
}
