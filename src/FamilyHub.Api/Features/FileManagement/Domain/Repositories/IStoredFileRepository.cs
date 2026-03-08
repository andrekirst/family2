using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IStoredFileRepository : IWriteRepository<Entities.StoredFile, FileId>
{
    Task<List<Entities.StoredFile>> GetByFolderIdAsync(FolderId folderId, CancellationToken ct = default);
    Task<List<Entities.StoredFile>> GetByFolderIdsAsync(IEnumerable<FolderId> folderIds, CancellationToken ct = default);
    Task<List<Entities.StoredFile>> GetByIdsAsync(IEnumerable<FileId> ids, CancellationToken ct = default);
    Task<List<Entities.StoredFile>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task RemoveAsync(Entities.StoredFile file, CancellationToken ct = default);
    Task RemoveRangeAsync(IEnumerable<Entities.StoredFile> files, CancellationToken ct = default);
}
