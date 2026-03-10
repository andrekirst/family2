using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeSavedSearchRepository : ISavedSearchRepository
{
    public List<SavedSearch> Searches { get; } = [];

    public Task<SavedSearch?> GetByIdAsync(SavedSearchId id, CancellationToken cancellationToken = default)
        => Task.FromResult(Searches.FirstOrDefault(s => s.Id == id));

    public Task<bool> ExistsByIdAsync(SavedSearchId id, CancellationToken cancellationToken = default)
        => Task.FromResult(Searches.Any(s => s.Id == id));

    public Task<List<SavedSearch>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
        => Task.FromResult(Searches
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToList());

    public Task AddAsync(SavedSearch search, CancellationToken cancellationToken = default)
    {
        Searches.Add(search);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(SavedSearch search, CancellationToken cancellationToken = default)
    {
        Searches.Remove(search);
        return Task.CompletedTask;
    }
}
