using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IStoredFileRepository
{
    Task<Entities.StoredFile?> GetByIdAsync(FileId id, CancellationToken ct = default);
    Task<List<Entities.StoredFile>> GetByFolderIdAsync(FolderId folderId, CancellationToken ct = default);
    Task<List<Entities.StoredFile>> GetByFolderIdsAsync(IEnumerable<FolderId> folderIds, CancellationToken ct = default);
    Task<List<Entities.StoredFile>> GetByIdsAsync(IEnumerable<FileId> ids, CancellationToken ct = default);
    Task AddAsync(Entities.StoredFile file, CancellationToken ct = default);
    Task RemoveAsync(Entities.StoredFile file, CancellationToken ct = default);
    Task RemoveRangeAsync(IEnumerable<Entities.StoredFile> files, CancellationToken ct = default);
}
