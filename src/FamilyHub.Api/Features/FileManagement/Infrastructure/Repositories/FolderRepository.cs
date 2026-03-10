using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class FolderRepository(AppDbContext context) : IFolderRepository
{
    public async Task<Folder?> GetByIdAsync(FolderId id, CancellationToken cancellationToken = default)
        => await context.Set<Folder>().FindAsync([id], cancellationToken: cancellationToken);

    public async Task<bool> ExistsByIdAsync(FolderId id, CancellationToken cancellationToken = default)
        => await context.Set<Folder>().AnyAsync(f => f.Id == id, cancellationToken);

    public async Task<Folder?> GetRootFolderAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<Folder>()
            .FirstOrDefaultAsync(f => f.FamilyId == familyId && f.ParentFolderId == null, cancellationToken);

    public async Task<Folder?> GetInboxFolderAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<Folder>()
            .FirstOrDefaultAsync(f => f.FamilyId == familyId && f.IsInbox, cancellationToken);

    public async Task<List<Folder>> GetChildrenAsync(FolderId parentId, CancellationToken cancellationToken = default)
        => await context.Set<Folder>()
            .Where(f => f.ParentFolderId == parentId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);

    public async Task<List<Folder>> GetDescendantsAsync(string materializedPathPrefix, FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<Folder>()
            .Where(f => f.FamilyId == familyId && f.MaterializedPath.StartsWith(materializedPathPrefix))
            .OrderBy(f => f.MaterializedPath)
            .ToListAsync(cancellationToken);

    public async Task<List<Folder>> GetAncestorsAsync(FolderId folderId, CancellationToken cancellationToken = default)
    {
        var folder = await context.Set<Folder>().FindAsync([folderId], cancellationToken: cancellationToken);
        if (folder is null)
        {
            return [];
        }

        // Parse folder IDs from materialized path (e.g., "/id1/id2/" → [id1, id2])
        var pathSegments = folder.MaterializedPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Guid.TryParse(s, out var g) ? (Guid?)g : null)
            .Where(g => g.HasValue)
            .Select(g => FolderId.From(g!.Value))
            .ToList();

        if (pathSegments.Count == 0)
        {
            return [];
        }

        var ancestors = await context.Set<Folder>()
            .Where(f => pathSegments.Contains(f.Id))
            .ToListAsync(cancellationToken);

        // Order by position in path (root first)
        return pathSegments
            .Select(id => ancestors.FirstOrDefault(a => a.Id == id))
            .Where(f => f is not null)
            .ToList()!;
    }

    public async Task<long> GetTotalFileSizeAsync(FolderId folderId, string materializedPathPrefix, FamilyId familyId, CancellationToken cancellationToken = default)
    {
        // Get all folder IDs in the subtree (including the folder itself)
        var descendantFolderIds = await context.Set<Folder>()
            .Where(f => f.FamilyId == familyId &&
                (f.Id == folderId || f.MaterializedPath.StartsWith(materializedPathPrefix)))
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        if (descendantFolderIds.Count == 0)
        {
            return 0;
        }

        return await context.Set<StoredFile>()
            .Where(f => descendantFolderIds.Contains(f.FolderId))
            .SumAsync(f => f.Size.Value, cancellationToken);
    }

    public async Task<int> GetDescendantCountAsync(string materializedPathPrefix, FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<Folder>()
            .CountAsync(f => f.FamilyId == familyId && f.MaterializedPath.StartsWith(materializedPathPrefix), cancellationToken);

    public async Task AddAsync(Folder folder, CancellationToken cancellationToken = default)
        => await context.Set<Folder>().AddAsync(folder, cancellationToken);

    public Task RemoveAsync(Folder folder, CancellationToken cancellationToken = default)
    {
        context.Set<Folder>().Remove(folder);
        return Task.CompletedTask;
    }

    public Task RemoveRangeAsync(IEnumerable<Folder> folders, CancellationToken cancellationToken = default)
    {
        context.Set<Folder>().RemoveRange(folders);
        return Task.CompletedTask;
    }
}
