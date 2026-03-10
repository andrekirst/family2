using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class SavedSearchRepository(AppDbContext context) : ISavedSearchRepository
{
    public async Task<SavedSearch?> GetByIdAsync(SavedSearchId id, CancellationToken cancellationToken = default)
        => await context.Set<SavedSearch>().FindAsync([id], cancellationToken: cancellationToken);

    public async Task<bool> ExistsByIdAsync(SavedSearchId id, CancellationToken cancellationToken = default)
        => await context.Set<SavedSearch>().AnyAsync(s => s.Id == id, cancellationToken);

    public async Task<List<SavedSearch>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
        => await context.Set<SavedSearch>()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(SavedSearch search, CancellationToken cancellationToken = default)
        => await context.Set<SavedSearch>().AddAsync(search, cancellationToken);

    public Task RemoveAsync(SavedSearch search, CancellationToken cancellationToken = default)
    {
        context.Set<SavedSearch>().Remove(search);
        return Task.CompletedTask;
    }
}
