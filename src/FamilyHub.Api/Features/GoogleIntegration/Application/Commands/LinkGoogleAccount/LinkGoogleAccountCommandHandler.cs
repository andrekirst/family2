using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.LinkGoogleAccount;

public sealed class LinkGoogleAccountCommandHandler(
    IOAuthStateRepository stateRepository,
    IGoogleAccountLinkRepository linkRepository,
    IGoogleOAuthService oauthService,
    ITokenEncryptionService encryptionService)
    : ICommandHandler<LinkGoogleAccountCommand, LinkGoogleAccountResult>
{
    public async ValueTask<LinkGoogleAccountResult> Handle(
        LinkGoogleAccountCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Validate state against OAuthState table
        var oauthState = await stateRepository.GetByStateAsync(command.State, cancellationToken)
            ?? throw new DomainException("Invalid or expired OAuth state");

        if (oauthState.IsExpired())
        {
            await stateRepository.DeleteAsync(oauthState, cancellationToken);
            await stateRepository.SaveChangesAsync(cancellationToken);
            throw new DomainException("OAuth state has expired. Please try linking again.");
        }

        var userId = oauthState.UserId;
        var codeVerifier = oauthState.CodeVerifier;

        // 2. Exchange authorization code for tokens
        var tokenResponse = await oauthService.ExchangeCodeForTokensAsync(
            command.Code, codeVerifier, cancellationToken);

        if (string.IsNullOrEmpty(tokenResponse.RefreshToken))
            throw new DomainException("Google did not provide a refresh token. Please try linking again.");

        // 3. Get user info from Google
        var userInfo = await oauthService.GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken);

        // 4. Check if user already has a linked account
        var existingLink = await linkRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existingLink is not null)
            throw new DomainException("A Google account is already linked. Unlink it first.");

        // 5. Encrypt tokens
        var encryptedAccessToken = EncryptedToken.From(
            encryptionService.Encrypt(tokenResponse.AccessToken));
        var encryptedRefreshToken = EncryptedToken.From(
            encryptionService.Encrypt(tokenResponse.RefreshToken));

        // 6. Create aggregate
        var link = GoogleAccountLink.Create(
            userId,
            GoogleAccountId.From(userInfo.Sub),
            Email.From(userInfo.Email),
            encryptedAccessToken,
            encryptedRefreshToken,
            DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
            GoogleScopes.From(tokenResponse.Scope));

        await linkRepository.AddAsync(link, cancellationToken);

        // 7. Clean up OAuth state
        await stateRepository.DeleteAsync(oauthState, cancellationToken);

        return new LinkGoogleAccountResult(true);
    }
}
