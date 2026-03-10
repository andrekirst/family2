using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class StoredFileRepository(AppDbContext context) : IStoredFileRepository
{
    public async Task<StoredFile?> GetByIdAsync(FileId id, CancellationToken cancellationToken = default)
        => await context.Set<StoredFile>().FindAsync([id], cancellationToken: cancellationToken);

    public async Task<bool> ExistsByIdAsync(FileId id, CancellationToken cancellationToken = default)
        => await context.Set<StoredFile>().AnyAsync(f => f.Id == id, cancellationToken);

    public async Task<List<StoredFile>> GetByFolderIdAsync(FolderId folderId, CancellationToken cancellationToken = default)
        => await context.Set<StoredFile>()
            .Where(f => f.FolderId == folderId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);

    public async Task<List<StoredFile>> GetByFolderIdsAsync(IEnumerable<FolderId> folderIds, CancellationToken cancellationToken = default)
    {
        var ids = folderIds.ToList();
        return await context.Set<StoredFile>()
            .Where(f => ids.Contains(f.FolderId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StoredFile>> GetByIdsAsync(IEnumerable<FileId> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await context.Set<StoredFile>()
            .Where(f => idList.Contains(f.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StoredFile>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<StoredFile>()
            .Where(f => f.FamilyId == familyId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(StoredFile file, CancellationToken cancellationToken = default)
        => await context.Set<StoredFile>().AddAsync(file, cancellationToken);

    public Task RemoveAsync(StoredFile file, CancellationToken cancellationToken = default)
    {
        context.Set<StoredFile>().Remove(file);
        return Task.CompletedTask;
    }

    public Task RemoveRangeAsync(IEnumerable<StoredFile> files, CancellationToken cancellationToken = default)
    {
        context.Set<StoredFile>().RemoveRange(files);
        return Task.CompletedTask;
    }
}
