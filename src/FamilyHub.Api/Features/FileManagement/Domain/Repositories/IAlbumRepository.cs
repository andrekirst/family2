using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IAlbumRepository
{
    Task<Entities.Album?> GetByIdAsync(AlbumId id, CancellationToken ct = default);
    Task<List<Entities.Album>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task AddAsync(Entities.Album album, CancellationToken ct = default);
    Task RemoveAsync(Entities.Album album, CancellationToken ct = default);
}
