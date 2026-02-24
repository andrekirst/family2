using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeSavedSearchRepository : ISavedSearchRepository
{
    public List<SavedSearch> Searches { get; } = [];

    public Task<SavedSearch?> GetByIdAsync(SavedSearchId id, CancellationToken ct = default)
        => Task.FromResult(Searches.FirstOrDefault(s => s.Id == id));

    public Task<List<SavedSearch>> GetByUserIdAsync(UserId userId, CancellationToken ct = default)
        => Task.FromResult(Searches
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToList());

    public Task AddAsync(SavedSearch search, CancellationToken ct = default)
    {
        Searches.Add(search);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(SavedSearch search, CancellationToken ct = default)
    {
        Searches.Remove(search);
        return Task.CompletedTask;
    }
}
