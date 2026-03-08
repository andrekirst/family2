using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class RecentSearchRepository(AppDbContext context) : IRecentSearchRepository
{
    public async Task<List<RecentSearch>> GetByUserIdAsync(UserId userId, int limit = 10, CancellationToken cancellationToken = default)
        => await context.Set<RecentSearch>()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.SearchedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(RecentSearch search, CancellationToken cancellationToken = default)
        => await context.Set<RecentSearch>().AddAsync(search, cancellationToken);

    public async Task RemoveOldestAsync(UserId userId, int keepCount = 10, CancellationToken cancellationToken = default)
    {
        var toRemove = await context.Set<RecentSearch>()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.SearchedAt)
            .Skip(keepCount)
            .ToListAsync(cancellationToken);

        if (toRemove.Count > 0)
        {
            context.Set<RecentSearch>().RemoveRange(toRemove);
        }
    }
}
