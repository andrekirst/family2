using FamilyHub.Api.Features.Photos.Domain.Repositories;
using FamilyHub.Api.Features.Photos.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakePhotoRepository(List<PhotoDto>? existingPhotos = null) : IPhotoRepository
{
    private readonly List<PhotoDto> _allPhotos = existingPhotos ?? [];

    public Task<PhotoDto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_allPhotos.FirstOrDefault(p => p.Id == id));

    public Task<List<PhotoDto>> GetByFamilyAsync(FamilyId familyId, int skip, int take, CancellationToken ct = default) =>
        Task.FromResult(_allPhotos
            .Where(p => p.FamilyId == familyId.Value)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList());

    public Task<int> GetCountByFamilyAsync(FamilyId familyId, CancellationToken ct = default) =>
        Task.FromResult(_allPhotos.Count(p => p.FamilyId == familyId.Value));

    public Task<PhotoDto?> GetNextAsync(FamilyId familyId, DateTime createdAt, Guid currentId, CancellationToken ct = default) =>
        Task.FromResult(_allPhotos
            .Where(p => p.FamilyId == familyId.Value)
            .Where(p => p.CreatedAt < createdAt || (p.CreatedAt == createdAt && p.Id.CompareTo(currentId) < 0))
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault());

    public Task<PhotoDto?> GetPreviousAsync(FamilyId familyId, DateTime createdAt, Guid currentId, CancellationToken ct = default) =>
        Task.FromResult(_allPhotos
            .Where(p => p.FamilyId == familyId.Value)
            .Where(p => p.CreatedAt > createdAt || (p.CreatedAt == createdAt && p.Id.CompareTo(currentId) > 0))
            .OrderBy(p => p.CreatedAt)
            .FirstOrDefault());
}
