using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class SavedSearchRepository(AppDbContext context) : ISavedSearchRepository
{
    public async Task<SavedSearch?> GetByIdAsync(SavedSearchId id, CancellationToken ct = default)
        => await context.Set<SavedSearch>().FindAsync([id], cancellationToken: ct);

    public async Task<List<SavedSearch>> GetByUserIdAsync(UserId userId, CancellationToken ct = default)
        => await context.Set<SavedSearch>()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(SavedSearch search, CancellationToken ct = default)
        => await context.Set<SavedSearch>().AddAsync(search, ct);

    public Task RemoveAsync(SavedSearch search, CancellationToken ct = default)
    {
        context.Set<SavedSearch>().Remove(search);
        return Task.CompletedTask;
    }
}
