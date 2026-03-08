using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Commands.LinkGoogleAccount;

[SecurityCheck("IDOR")]
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
        // 1. Retrieve OAuth state (existence guaranteed by validator)
        var oauthState = (await stateRepository.GetByStateAsync(command.State, cancellationToken))!;

        if (oauthState.IsExpired())
        {
            await stateRepository.DeleteAsync(oauthState, cancellationToken);
            throw new DomainException("OAuth state has expired. Please try linking again.");
        }

        var userId = oauthState.UserId;
        var codeVerifier = oauthState.CodeVerifier;

        // 2. Exchange authorization code for tokens
        var tokenResponse = await oauthService.ExchangeCodeForTokensAsync(
            command.Code, codeVerifier, cancellationToken);

        if (string.IsNullOrEmpty(tokenResponse.RefreshToken))
        {
            throw new DomainException("Google did not provide a refresh token. Please try linking again.");
        }

        // 3. Get user info from Google
        var userInfo = await oauthService.GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken);

        // 4. Encrypt tokens (already-linked check guaranteed by validator)
        var encryptedAccessToken = EncryptedToken.From(
            encryptionService.Encrypt(tokenResponse.AccessToken));
        var encryptedRefreshToken = EncryptedToken.From(
            encryptionService.Encrypt(tokenResponse.RefreshToken));

        // 5. Create aggregate
        var link = GoogleAccountLink.Create(
            userId,
            GoogleAccountId.From(userInfo.Sub),
            Email.From(userInfo.Email),
            encryptedAccessToken,
            encryptedRefreshToken,
            DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
            GoogleScopes.From(tokenResponse.Scope));

        await linkRepository.AddAsync(link, cancellationToken);

        // 6. Clean up OAuth state
        await stateRepository.DeleteAsync(oauthState, cancellationToken);

        return new LinkGoogleAccountResult(true);
    }
}
