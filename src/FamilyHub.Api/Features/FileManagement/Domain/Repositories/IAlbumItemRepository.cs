using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IAlbumItemRepository
{
    Task<List<Entities.AlbumItem>> GetByAlbumIdAsync(AlbumId albumId, CancellationToken ct = default);
    Task<bool> ExistsAsync(AlbumId albumId, FileId fileId, CancellationToken ct = default);
    Task AddAsync(Entities.AlbumItem item, CancellationToken ct = default);
    Task RemoveAsync(Entities.AlbumItem item, CancellationToken ct = default);
    Task RemoveByAlbumIdAsync(AlbumId albumId, CancellationToken ct = default);
    Task<int> GetItemCountAsync(AlbumId albumId, CancellationToken ct = default);
    Task<FileId?> GetFirstImageFileIdAsync(AlbumId albumId, CancellationToken ct = default);
}
