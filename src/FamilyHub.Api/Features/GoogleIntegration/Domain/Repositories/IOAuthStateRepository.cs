using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;

public interface IOAuthStateRepository
{
    Task<OAuthState?> GetByStateAsync(string state, CancellationToken cancellationToken = default);
    Task AddAsync(OAuthState oauthState, CancellationToken cancellationToken = default);
    Task DeleteAsync(OAuthState oauthState, CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
