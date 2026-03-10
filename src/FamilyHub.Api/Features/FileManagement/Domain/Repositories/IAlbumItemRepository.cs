using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IAlbumItemRepository
{
    Task<List<Entities.AlbumItem>> GetByAlbumIdAsync(AlbumId albumId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(AlbumId albumId, FileId fileId, CancellationToken cancellationToken = default);
    Task AddAsync(Entities.AlbumItem item, CancellationToken cancellationToken = default);
    Task RemoveAsync(Entities.AlbumItem item, CancellationToken cancellationToken = default);
    Task RemoveByAlbumIdAsync(AlbumId albumId, CancellationToken cancellationToken = default);
    Task<int> GetItemCountAsync(AlbumId albumId, CancellationToken cancellationToken = default);
    Task<FileId?> GetFirstImageFileIdAsync(AlbumId albumId, CancellationToken cancellationToken = default);
}
