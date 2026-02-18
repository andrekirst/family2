using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeRecentSearchRepository : IRecentSearchRepository
{
    public List<RecentSearch> Searches { get; } = [];

    public Task<List<RecentSearch>> GetByUserIdAsync(UserId userId, int limit = 10, CancellationToken ct = default)
        => Task.FromResult(Searches
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SearchedAt)
            .Take(limit)
            .ToList());

    public Task AddAsync(RecentSearch search, CancellationToken ct = default)
    {
        Searches.Add(search);
        return Task.CompletedTask;
    }

    public Task RemoveOldestAsync(UserId userId, int keepCount = 10, CancellationToken ct = default)
    {
        var toRemove = Searches
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SearchedAt)
            .Skip(keepCount)
            .ToList();

        foreach (var s in toRemove)
            Searches.Remove(s);

        return Task.CompletedTask;
    }
}
