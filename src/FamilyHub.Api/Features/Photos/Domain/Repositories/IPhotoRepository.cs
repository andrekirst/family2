using FamilyHub.Api.Features.Photos.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Photos.Domain.Repositories;

public interface IPhotoRepository
{
    Task<PhotoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<PhotoDto>> GetByFamilyAsync(FamilyId familyId, int skip, int take, CancellationToken ct = default);
    Task<int> GetCountByFamilyAsync(FamilyId familyId, CancellationToken ct = default);
    Task<PhotoDto?> GetNextAsync(FamilyId familyId, DateTime createdAt, Guid currentId, CancellationToken ct = default);
    Task<PhotoDto?> GetPreviousAsync(FamilyId familyId, DateTime createdAt, Guid currentId, CancellationToken ct = default);
}
