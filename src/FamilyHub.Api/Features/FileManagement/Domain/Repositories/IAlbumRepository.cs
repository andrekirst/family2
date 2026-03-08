using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IAlbumRepository : IWriteRepository<Entities.Album, AlbumId>
{
    Task<List<Entities.Album>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task RemoveAsync(Entities.Album album, CancellationToken ct = default);
}
