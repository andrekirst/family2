using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;

public interface IOAuthStateRepository
{
    Task<OAuthState?> GetByStateAsync(string state, CancellationToken ct = default);
    Task AddAsync(OAuthState oauthState, CancellationToken ct = default);
    Task DeleteAsync(OAuthState oauthState, CancellationToken ct = default);
    Task DeleteExpiredAsync(CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
