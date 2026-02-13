using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.UnlinkGoogleAccount;

public sealed class UnlinkGoogleAccountCommandHandler(
    IGoogleAccountLinkRepository linkRepository,
    IGoogleOAuthService oauthService,
    ITokenEncryptionService encryptionService)
    : ICommandHandler<UnlinkGoogleAccountCommand, bool>
{
    public async ValueTask<bool> Handle(
        UnlinkGoogleAccountCommand command,
        CancellationToken cancellationToken)
    {
        var link = await linkRepository.GetByUserIdAsync(command.UserId, cancellationToken)
            ?? throw new DomainException("No Google account linked");

        // Revoke token at Google (best-effort)
        try
        {
            var accessToken = encryptionService.Decrypt(link.EncryptedAccessToken.Value);
            await oauthService.RevokeTokenAsync(accessToken, cancellationToken);
        }
        catch
        {
            // Token revocation is best-effort — proceed with local deletion
        }

        // Mark as revoked (raises domain event) then hard-delete
        link.MarkRevoked();
        await linkRepository.DeleteAsync(link, cancellationToken);

        return true;
    }
}
