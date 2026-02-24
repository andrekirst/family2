using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class AlbumRepository(AppDbContext context) : IAlbumRepository
{
    public async Task<Album?> GetByIdAsync(AlbumId id, CancellationToken ct = default)
        => await context.Set<Album>().FindAsync([id], cancellationToken: ct);

    public async Task<List<Album>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => await context.Set<Album>()
            .Where(a => a.FamilyId == familyId)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Album album, CancellationToken ct = default)
        => await context.Set<Album>().AddAsync(album, ct);

    public Task RemoveAsync(Album album, CancellationToken ct = default)
    {
        context.Set<Album>().Remove(album);
        return Task.CompletedTask;
    }
}
