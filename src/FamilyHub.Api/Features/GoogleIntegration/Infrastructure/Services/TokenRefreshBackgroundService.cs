using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Models;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;

public sealed class TokenRefreshBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<TokenRefreshOptions> options,
    ILogger<TokenRefreshBackgroundService> logger) : BackgroundService
{
    private readonly TokenRefreshOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Google token refresh service started. Interval: {Interval}min, refresh threshold: {Threshold}min",
            _options.IntervalMinutes, _options.RefreshBeforeExpiryMinutes);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await RefreshExpiringTokensAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error during Google token refresh cycle");
            }
        }
    }

    private async Task RefreshExpiringTokensAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var linkRepository = scope.ServiceProvider.GetRequiredService<IGoogleAccountLinkRepository>();
        var oauthService = scope.ServiceProvider.GetRequiredService<IGoogleOAuthService>();
        var encryptionService = scope.ServiceProvider.GetRequiredService<ITokenEncryptionService>();

        var expiringBefore = DateTime.UtcNow.AddMinutes(_options.RefreshBeforeExpiryMinutes);
        var expiringLinks = await linkRepository.GetExpiringTokensAsync(expiringBefore, ct);

        if (expiringLinks.Count == 0) return;

        logger.LogInformation("Found {Count} Google tokens to refresh", expiringLinks.Count);

        foreach (var link in expiringLinks)
        {
            try
            {
                var refreshToken = encryptionService.Decrypt(link.EncryptedRefreshToken.Value);
                var tokenResponse = await oauthService.RefreshAccessTokenAsync(refreshToken, ct);

                var encryptedAccessToken = EncryptedToken.From(
                    encryptionService.Encrypt(tokenResponse.AccessToken));
                var newExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

                link.RefreshAccessToken(encryptedAccessToken, newExpiresAt);
                await linkRepository.UpdateAsync(link, ct);

                logger.LogInformation(
                    "Refreshed Google token for user {UserId}, new expiry: {ExpiresAt}",
                    link.UserId, newExpiresAt);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to refresh Google token for user {UserId}", link.UserId);
                link.MarkRefreshFailed(ex.Message);
                await linkRepository.UpdateAsync(link, ct);
            }
        }

        await linkRepository.SaveChangesAsync(ct);
    }
}
