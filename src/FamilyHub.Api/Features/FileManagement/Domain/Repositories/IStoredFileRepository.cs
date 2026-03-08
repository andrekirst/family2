using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IStoredFileRepository : IWriteRepository<Entities.StoredFile, FileId>
{
    Task<List<Entities.StoredFile>> GetByFolderIdAsync(FolderId folderId, CancellationToken cancellationToken = default);
    Task<List<Entities.StoredFile>> GetByFolderIdsAsync(IEnumerable<FolderId> folderIds, CancellationToken cancellationToken = default);
    Task<List<Entities.StoredFile>> GetByIdsAsync(IEnumerable<FileId> ids, CancellationToken cancellationToken = default);
    Task<List<Entities.StoredFile>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);
    Task RemoveAsync(Entities.StoredFile file, CancellationToken cancellationToken = default);
    Task RemoveRangeAsync(IEnumerable<Entities.StoredFile> files, CancellationToken cancellationToken = default);
}
