using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFolderRepository
{
    Task<Entities.Folder?> GetByIdAsync(FolderId id, CancellationToken ct = default);
    Task<Entities.Folder?> GetRootFolderAsync(FamilyId familyId, CancellationToken ct = default);
    Task<Entities.Folder?> GetInboxFolderAsync(FamilyId familyId, CancellationToken ct = default);
    Task<List<Entities.Folder>> GetChildrenAsync(FolderId parentId, CancellationToken ct = default);

    /// <summary>
    /// Gets all descendants of a folder using materialized path prefix matching.
    /// </summary>
    Task<List<Entities.Folder>> GetDescendantsAsync(string materializedPathPrefix, FamilyId familyId, CancellationToken ct = default);

    /// <summary>
    /// Gets the ancestor chain from root to the given folder (for breadcrumbs).
    /// Returns folders ordered from root to the target folder.
    /// </summary>
    Task<List<Entities.Folder>> GetAncestorsAsync(FolderId folderId, CancellationToken ct = default);

    /// <summary>
    /// Counts files recursively in a folder and its descendants.
    /// </summary>
    Task<long> GetTotalFileSizeAsync(FolderId folderId, string materializedPathPrefix, FamilyId familyId, CancellationToken ct = default);

    /// <summary>
    /// Counts all items (files + subfolders) in a folder and its descendants.
    /// </summary>
    Task<int> GetDescendantCountAsync(string materializedPathPrefix, FamilyId familyId, CancellationToken ct = default);

    Task AddAsync(Entities.Folder folder, CancellationToken ct = default);
    Task RemoveAsync(Entities.Folder folder, CancellationToken ct = default);
    Task RemoveRangeAsync(IEnumerable<Entities.Folder> folders, CancellationToken ct = default);
}
