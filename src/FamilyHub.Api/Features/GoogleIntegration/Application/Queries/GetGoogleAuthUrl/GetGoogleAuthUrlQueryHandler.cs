using FamilyHub.Common.Application;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetGoogleAuthUrl;

public sealed class GetGoogleAuthUrlQueryHandler(
    IGoogleOAuthService oauthService,
    IOAuthStateRepository stateRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IQueryHandler<GetGoogleAuthUrlQuery, string>
{
    public async ValueTask<string> Handle(
        GetGoogleAuthUrlQuery query,
        CancellationToken cancellationToken)
    {
        var (authUrl, state, codeVerifier) = oauthService.BuildConsentUrl();

        var oauthState = OAuthState.Create(state, query.UserId, codeVerifier, timeProvider.GetUtcNow());
        await stateRepository.AddAsync(oauthState, cancellationToken);
        // Explicit save — queries skip TransactionBehavior
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return authUrl;
    }
}
