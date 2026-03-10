using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class AlbumRepository(AppDbContext context) : IAlbumRepository
{
    public async Task<Album?> GetByIdAsync(AlbumId id, CancellationToken cancellationToken = default)
        => await context.Set<Album>().FindAsync([id], cancellationToken: cancellationToken);

    public async Task<bool> ExistsByIdAsync(AlbumId id, CancellationToken cancellationToken = default)
        => await context.Set<Album>().AnyAsync(a => a.Id == id, cancellationToken);

    public async Task<List<Album>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<Album>()
            .Where(a => a.FamilyId == familyId)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Album album, CancellationToken cancellationToken = default)
        => await context.Set<Album>().AddAsync(album, cancellationToken);

    public Task RemoveAsync(Album album, CancellationToken cancellationToken = default)
    {
        context.Set<Album>().Remove(album);
        return Task.CompletedTask;
    }
}
