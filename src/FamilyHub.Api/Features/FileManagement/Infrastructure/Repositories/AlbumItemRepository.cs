using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class AlbumItemRepository(AppDbContext context) : IAlbumItemRepository
{
    public async Task<List<AlbumItem>> GetByAlbumIdAsync(AlbumId albumId, CancellationToken cancellationToken = default)
        => await context.Set<AlbumItem>()
            .Where(ai => ai.AlbumId == albumId)
            .OrderByDescending(ai => ai.AddedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(AlbumId albumId, FileId fileId, CancellationToken cancellationToken = default)
        => await context.Set<AlbumItem>()
            .AnyAsync(ai => ai.AlbumId == albumId && ai.FileId == fileId, cancellationToken);

    public async Task AddAsync(AlbumItem item, CancellationToken cancellationToken = default)
        => await context.Set<AlbumItem>().AddAsync(item, cancellationToken);

    public Task RemoveAsync(AlbumItem item, CancellationToken cancellationToken = default)
    {
        context.Set<AlbumItem>().Remove(item);
        return Task.CompletedTask;
    }

    public async Task RemoveByAlbumIdAsync(AlbumId albumId, CancellationToken cancellationToken = default)
    {
        var items = await context.Set<AlbumItem>()
            .Where(ai => ai.AlbumId == albumId)
            .ToListAsync(cancellationToken);
        context.Set<AlbumItem>().RemoveRange(items);
    }

    public async Task<int> GetItemCountAsync(AlbumId albumId, CancellationToken cancellationToken = default)
        => await context.Set<AlbumItem>()
            .CountAsync(ai => ai.AlbumId == albumId, cancellationToken);

    public async Task<FileId?> GetFirstImageFileIdAsync(AlbumId albumId, CancellationToken cancellationToken = default)
    {
        // Returns the earliest-added file in the album (cover auto-selection)
        var item = await context.Set<AlbumItem>()
            .Where(ai => ai.AlbumId == albumId)
            .OrderBy(ai => ai.AddedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return item?.FileId;
    }
}
