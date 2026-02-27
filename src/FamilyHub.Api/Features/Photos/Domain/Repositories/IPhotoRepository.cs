using FamilyHub.Api.Features.Photos.Domain.Entities;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Photos.Domain.Repositories;

public interface IPhotoRepository
{
    Task<Photo?> GetByIdAsync(PhotoId id, CancellationToken ct = default);
    Task<List<Photo>> GetByFamilyAsync(FamilyId familyId, int skip, int take, CancellationToken ct = default);
    Task<int> GetCountByFamilyAsync(FamilyId familyId, CancellationToken ct = default);
    Task<Photo?> GetNextAsync(FamilyId familyId, DateTime createdAt, PhotoId currentId, CancellationToken ct = default);
    Task<Photo?> GetPreviousAsync(FamilyId familyId, DateTime createdAt, PhotoId currentId, CancellationToken ct = default);
    Task AddAsync(Photo photo, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
