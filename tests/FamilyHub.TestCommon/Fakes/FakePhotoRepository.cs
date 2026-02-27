using FamilyHub.Api.Features.Photos.Domain.Entities;
using FamilyHub.Api.Features.Photos.Domain.Repositories;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakePhotoRepository(Photo? existingPhoto = null) : IPhotoRepository
{
    public List<Photo> AddedPhotos { get; } = [];
    private readonly List<Photo> _allPhotos = existingPhoto is not null ? [existingPhoto] : [];

    public Task<Photo?> GetByIdAsync(PhotoId id, CancellationToken ct = default) =>
        Task.FromResult(_allPhotos.FirstOrDefault(p => p.Id == id && !p.IsDeleted));

    public Task<List<Photo>> GetByFamilyAsync(FamilyId familyId, int skip, int take, CancellationToken ct = default) =>
        Task.FromResult(_allPhotos
            .Where(p => p.FamilyId == familyId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList());

    public Task<int> GetCountByFamilyAsync(FamilyId familyId, CancellationToken ct = default) =>
        Task.FromResult(_allPhotos.Count(p => p.FamilyId == familyId && !p.IsDeleted));

    public Task<Photo?> GetNextAsync(FamilyId familyId, DateTime createdAt, PhotoId currentId, CancellationToken ct = default) =>
        Task.FromResult(_allPhotos
            .Where(p => p.FamilyId == familyId && !p.IsDeleted)
            .Where(p => p.CreatedAt < createdAt || (p.CreatedAt == createdAt && p.Id.Value.CompareTo(currentId.Value) < 0))
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault());

    public Task<Photo?> GetPreviousAsync(FamilyId familyId, DateTime createdAt, PhotoId currentId, CancellationToken ct = default) =>
        Task.FromResult(_allPhotos
            .Where(p => p.FamilyId == familyId && !p.IsDeleted)
            .Where(p => p.CreatedAt > createdAt || (p.CreatedAt == createdAt && p.Id.Value.CompareTo(currentId.Value) > 0))
            .OrderBy(p => p.CreatedAt)
            .FirstOrDefault());

    public Task AddAsync(Photo photo, CancellationToken ct = default)
    {
        AddedPhotos.Add(photo);
        _allPhotos.Add(photo);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Task.FromResult(1);
}
