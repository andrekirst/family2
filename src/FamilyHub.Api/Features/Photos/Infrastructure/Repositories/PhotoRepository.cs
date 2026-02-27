using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Photos.Domain.Entities;
using FamilyHub.Api.Features.Photos.Domain.Repositories;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Photos.Infrastructure.Repositories;

public sealed class PhotoRepository(AppDbContext context) : IPhotoRepository
{
    public async Task<Photo?> GetByIdAsync(PhotoId id, CancellationToken ct = default)
    {
        return await context.Photos
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
    }

    public async Task<List<Photo>> GetByFamilyAsync(
        FamilyId familyId, int skip, int take, CancellationToken ct = default)
    {
        return await context.Photos
            .Where(p => p.FamilyId == familyId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountByFamilyAsync(FamilyId familyId, CancellationToken ct = default)
    {
        return await context.Photos
            .CountAsync(p => p.FamilyId == familyId && !p.IsDeleted, ct);
    }

    public async Task<Photo?> GetNextAsync(
        FamilyId familyId, DateTime createdAt, PhotoId currentId, CancellationToken ct = default)
    {
        return await context.Photos
            .Where(p => p.FamilyId == familyId && !p.IsDeleted)
            .Where(p => p.CreatedAt < createdAt
                || (p.CreatedAt == createdAt && p.Id.Value.CompareTo(currentId.Value) < 0))
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Photo?> GetPreviousAsync(
        FamilyId familyId, DateTime createdAt, PhotoId currentId, CancellationToken ct = default)
    {
        return await context.Photos
            .Where(p => p.FamilyId == familyId && !p.IsDeleted)
            .Where(p => p.CreatedAt > createdAt
                || (p.CreatedAt == createdAt && p.Id.Value.CompareTo(currentId.Value) > 0))
            .OrderBy(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Photo photo, CancellationToken ct = default)
    {
        await context.Photos.AddAsync(photo, ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}
