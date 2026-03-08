using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface ISavedSearchRepository : IWriteRepository<SavedSearch, SavedSearchId>
{
    Task<List<SavedSearch>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task RemoveAsync(SavedSearch search, CancellationToken cancellationToken = default);
}
