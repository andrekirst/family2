using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IRecentSearchRepository
{
    Task<List<RecentSearch>> GetByUserIdAsync(UserId userId, int limit = 10, CancellationToken ct = default);
    Task AddAsync(RecentSearch search, CancellationToken ct = default);
    Task RemoveOldestAsync(UserId userId, int keepCount = 10, CancellationToken ct = default);
}
