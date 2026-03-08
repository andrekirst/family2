using FamilyHub.Api.Features.Photos.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Photos.Domain.Repositories;

public interface IPhotoRepository
{
    Task<PhotoDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<PhotoDto>> GetByFamilyAsync(FamilyId familyId, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> GetCountByFamilyAsync(FamilyId familyId, CancellationToken cancellationToken = default);
    Task<PhotoDto?> GetNextAsync(FamilyId familyId, DateTime createdAt, Guid currentId, CancellationToken cancellationToken = default);
    Task<PhotoDto?> GetPreviousAsync(FamilyId familyId, DateTime createdAt, Guid currentId, CancellationToken cancellationToken = default);
}
