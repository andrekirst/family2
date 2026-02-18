using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class AlbumItemRepository(AppDbContext context) : IAlbumItemRepository
{
    public async Task<List<AlbumItem>> GetByAlbumIdAsync(AlbumId albumId, CancellationToken ct = default)
        => await context.Set<AlbumItem>()
            .Where(ai => ai.AlbumId == albumId)
            .OrderByDescending(ai => ai.AddedAt)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(AlbumId albumId, FileId fileId, CancellationToken ct = default)
        => await context.Set<AlbumItem>()
            .AnyAsync(ai => ai.AlbumId == albumId && ai.FileId == fileId, ct);

    public async Task AddAsync(AlbumItem item, CancellationToken ct = default)
        => await context.Set<AlbumItem>().AddAsync(item, ct);

    public Task RemoveAsync(AlbumItem item, CancellationToken ct = default)
    {
        context.Set<AlbumItem>().Remove(item);
        return Task.CompletedTask;
    }

    public async Task RemoveByAlbumIdAsync(AlbumId albumId, CancellationToken ct = default)
    {
        var items = await context.Set<AlbumItem>()
            .Where(ai => ai.AlbumId == albumId)
            .ToListAsync(ct);
        context.Set<AlbumItem>().RemoveRange(items);
    }

    public async Task<int> GetItemCountAsync(AlbumId albumId, CancellationToken ct = default)
        => await context.Set<AlbumItem>()
            .CountAsync(ai => ai.AlbumId == albumId, ct);

    public async Task<FileId?> GetFirstImageFileIdAsync(AlbumId albumId, CancellationToken ct = default)
    {
        // Returns the earliest-added file in the album (cover auto-selection)
        var item = await context.Set<AlbumItem>()
            .Where(ai => ai.AlbumId == albumId)
            .OrderBy(ai => ai.AddedAt)
            .FirstOrDefaultAsync(ct);
        return item?.FileId;
    }
}
