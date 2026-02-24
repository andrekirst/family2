using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeAlbumItemRepository : IAlbumItemRepository
{
    public List<AlbumItem> Items { get; } = [];

    public Task<List<AlbumItem>> GetByAlbumIdAsync(AlbumId albumId, CancellationToken ct = default)
        => Task.FromResult(Items.Where(ai => ai.AlbumId == albumId).OrderByDescending(ai => ai.AddedAt).ToList());

    public Task<bool> ExistsAsync(AlbumId albumId, FileId fileId, CancellationToken ct = default)
        => Task.FromResult(Items.Any(ai => ai.AlbumId == albumId && ai.FileId == fileId));

    public Task AddAsync(AlbumItem item, CancellationToken ct = default)
    {
        Items.Add(item);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(AlbumItem item, CancellationToken ct = default)
    {
        var existing = Items.FirstOrDefault(ai => ai.AlbumId == item.AlbumId && ai.FileId == item.FileId);
        if (existing is not null) Items.Remove(existing);
        return Task.CompletedTask;
    }

    public Task RemoveByAlbumIdAsync(AlbumId albumId, CancellationToken ct = default)
    {
        Items.RemoveAll(ai => ai.AlbumId == albumId);
        return Task.CompletedTask;
    }

    public Task<int> GetItemCountAsync(AlbumId albumId, CancellationToken ct = default)
        => Task.FromResult(Items.Count(ai => ai.AlbumId == albumId));

    public Task<FileId?> GetFirstImageFileIdAsync(AlbumId albumId, CancellationToken ct = default)
    {
        var item = Items
            .Where(ai => ai.AlbumId == albumId)
            .OrderBy(ai => ai.AddedAt)
            .FirstOrDefault();
        return Task.FromResult(item?.FileId);
    }
}
