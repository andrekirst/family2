using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;
using FamilyHub.Api.Features.GoogleIntegration.Models;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.RefreshGoogleToken;

public sealed class RefreshGoogleTokenCommandHandler(
    IGoogleAccountLinkRepository linkRepository,
    IGoogleOAuthService oauthService,
    ITokenEncryptionService encryptionService)
    : ICommandHandler<RefreshGoogleTokenCommand, RefreshTokenResultDto>
{
    public async ValueTask<RefreshTokenResultDto> Handle(
        RefreshGoogleTokenCommand command,
        CancellationToken cancellationToken)
    {
        var link = await linkRepository.GetByUserIdAsync(command.UserId, cancellationToken)
            ?? throw new DomainException("No Google account linked");

        try
        {
            // Decrypt refresh token
            var refreshToken = encryptionService.Decrypt(link.EncryptedRefreshToken.Value);

            // Request new access token from Google
            var tokenResponse = await oauthService.RefreshAccessTokenAsync(refreshToken, cancellationToken);

            // Encrypt new access token
            var encryptedAccessToken = EncryptedToken.From(
                encryptionService.Encrypt(tokenResponse.AccessToken));

            var newExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            // Update aggregate
            link.RefreshAccessToken(encryptedAccessToken, newExpiresAt);
            await linkRepository.UpdateAsync(link, cancellationToken);

            return new RefreshTokenResultDto(true, newExpiresAt);
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            link.MarkRefreshFailed(ex.Message);
            await linkRepository.UpdateAsync(link, cancellationToken);
            return new RefreshTokenResultDto(false, null);
        }
    }
}
