using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFolderRepository : IWriteRepository<Entities.Folder, FolderId>
{
    Task<Entities.Folder?> GetRootFolderAsync(FamilyId familyId, CancellationToken cancellationToken = default);
    Task<Entities.Folder?> GetInboxFolderAsync(FamilyId familyId, CancellationToken cancellationToken = default);
    Task<List<Entities.Folder>> GetChildrenAsync(FolderId parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all descendants of a folder using materialized path prefix matching.
    /// </summary>
    Task<List<Entities.Folder>> GetDescendantsAsync(string materializedPathPrefix, FamilyId familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the ancestor chain from root to the given folder (for breadcrumbs).
    /// Returns folders ordered from root to the target folder.
    /// </summary>
    Task<List<Entities.Folder>> GetAncestorsAsync(FolderId folderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts files recursively in a folder and its descendants.
    /// </summary>
    Task<long> GetTotalFileSizeAsync(FolderId folderId, string materializedPathPrefix, FamilyId familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts all items (files + subfolders) in a folder and its descendants.
    /// </summary>
    Task<int> GetDescendantCountAsync(string materializedPathPrefix, FamilyId familyId, CancellationToken cancellationToken = default);

    Task RemoveAsync(Entities.Folder folder, CancellationToken cancellationToken = default);
    Task RemoveRangeAsync(IEnumerable<Entities.Folder> folders, CancellationToken cancellationToken = default);
}
