using FamilyHub.Api.Features.GoogleIntegration.Models;

namespace FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;

public interface IGoogleOAuthService
{
    (string AuthUrl, string State, string CodeVerifier) BuildConsentUrl();
    Task<GoogleTokenResponse> ExchangeCodeForTokensAsync(string code, string codeVerifier, CancellationToken cancellationToken = default);
    Task<GoogleTokenResponse> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<GoogleUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default);
}
