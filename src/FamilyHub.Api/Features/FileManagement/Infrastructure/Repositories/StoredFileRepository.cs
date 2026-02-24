using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class StoredFileRepository(AppDbContext context) : IStoredFileRepository
{
    public async Task<StoredFile?> GetByIdAsync(FileId id, CancellationToken ct = default)
        => await context.Set<StoredFile>().FindAsync([id], cancellationToken: ct);

    public async Task<List<StoredFile>> GetByFolderIdAsync(FolderId folderId, CancellationToken ct = default)
        => await context.Set<StoredFile>()
            .Where(f => f.FolderId == folderId)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);

    public async Task<List<StoredFile>> GetByFolderIdsAsync(IEnumerable<FolderId> folderIds, CancellationToken ct = default)
    {
        var ids = folderIds.ToList();
        return await context.Set<StoredFile>()
            .Where(f => ids.Contains(f.FolderId))
            .ToListAsync(ct);
    }

    public async Task<List<StoredFile>> GetByIdsAsync(IEnumerable<FileId> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await context.Set<StoredFile>()
            .Where(f => idList.Contains(f.Id))
            .ToListAsync(ct);
    }

    public async Task<List<StoredFile>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => await context.Set<StoredFile>()
            .Where(f => f.FamilyId == familyId)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);

    public async Task AddAsync(StoredFile file, CancellationToken ct = default)
        => await context.Set<StoredFile>().AddAsync(file, ct);

    public Task RemoveAsync(StoredFile file, CancellationToken ct = default)
    {
        context.Set<StoredFile>().Remove(file);
        return Task.CompletedTask;
    }

    public Task RemoveRangeAsync(IEnumerable<StoredFile> files, CancellationToken ct = default)
    {
        context.Set<StoredFile>().RemoveRange(files);
        return Task.CompletedTask;
    }
}
