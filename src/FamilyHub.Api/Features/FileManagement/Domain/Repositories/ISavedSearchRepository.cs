using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface ISavedSearchRepository
{
    Task<SavedSearch?> GetByIdAsync(SavedSearchId id, CancellationToken ct = default);
    Task<List<SavedSearch>> GetByUserIdAsync(UserId userId, CancellationToken ct = default);
    Task AddAsync(SavedSearch search, CancellationToken ct = default);
    Task RemoveAsync(SavedSearch search, CancellationToken ct = default);
}
