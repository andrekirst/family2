using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;

public interface IGoogleAccountLinkRepository
{
    Task<GoogleAccountLink?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<GoogleAccountLink?> GetByGoogleAccountIdAsync(GoogleAccountId googleAccountId, CancellationToken cancellationToken = default);
    Task<List<GoogleAccountLink>> GetExpiringTokensAsync(DateTime expiringBefore, CancellationToken cancellationToken = default);
    Task AddAsync(GoogleAccountLink link, CancellationToken cancellationToken = default);
    Task UpdateAsync(GoogleAccountLink link, CancellationToken cancellationToken = default);
    Task DeleteAsync(GoogleAccountLink link, CancellationToken cancellationToken = default);
}
