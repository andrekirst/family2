using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFolderRepository : IFolderRepository
{
    public List<Folder> Folders { get; } = [];

    public Task<Folder?> GetByIdAsync(FolderId id, CancellationToken ct = default)
        => Task.FromResult(Folders.FirstOrDefault(f => f.Id == id));

    public Task<Folder?> GetRootFolderAsync(FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Folders.FirstOrDefault(f => f.FamilyId == familyId && f.ParentFolderId == null));

    public Task<List<Folder>> GetChildrenAsync(FolderId parentId, CancellationToken ct = default)
        => Task.FromResult(Folders.Where(f => f.ParentFolderId == parentId).ToList());

    public Task<List<Folder>> GetDescendantsAsync(string materializedPathPrefix, FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Folders
            .Where(f => f.FamilyId == familyId
                && f.MaterializedPath.StartsWith(materializedPathPrefix))
            .OrderBy(f => f.MaterializedPath)
            .ToList());

    public Task<List<Folder>> GetAncestorsAsync(FolderId folderId, CancellationToken ct = default)
    {
        var folder = Folders.FirstOrDefault(f => f.Id == folderId);
        if (folder is null)
            return Task.FromResult(new List<Folder>());

        var pathSegments = folder.MaterializedPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Guid.TryParse(s, out var g) ? (Guid?)g : null)
            .Where(g => g.HasValue)
            .Select(g => FolderId.From(g!.Value))
            .ToList();

        var ancestors = pathSegments
            .Select(id => Folders.FirstOrDefault(f => f.Id == id))
            .Where(f => f is not null)
            .ToList()!;

        return Task.FromResult(ancestors!);
    }

    public Task<long> GetTotalFileSizeAsync(FolderId folderId, string materializedPathPrefix, FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(0L); // Simplified for tests — override in specific tests if needed

    public Task<int> GetDescendantCountAsync(string materializedPathPrefix, FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Folders
            .Count(f => f.FamilyId == familyId
                && f.MaterializedPath.StartsWith(materializedPathPrefix)));

    public Task AddAsync(Folder folder, CancellationToken ct = default)
    {
        Folders.Add(folder);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Folder folder, CancellationToken ct = default)
    {
        Folders.Remove(folder);
        return Task.CompletedTask;
    }

    public Task RemoveRangeAsync(IEnumerable<Folder> folders, CancellationToken ct = default)
    {
        foreach (var folder in folders.ToList())
            Folders.Remove(folder);
        return Task.CompletedTask;
    }
}
