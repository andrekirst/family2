using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;

public interface IGoogleAccountLinkRepository
{
    Task<GoogleAccountLink?> GetByUserIdAsync(UserId userId, CancellationToken ct = default);
    Task<GoogleAccountLink?> GetByGoogleAccountIdAsync(GoogleAccountId googleAccountId, CancellationToken ct = default);
    Task<List<GoogleAccountLink>> GetExpiringTokensAsync(DateTime expiringBefore, CancellationToken ct = default);
    Task AddAsync(GoogleAccountLink link, CancellationToken ct = default);
    Task UpdateAsync(GoogleAccountLink link, CancellationToken ct = default);
    Task DeleteAsync(GoogleAccountLink link, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
