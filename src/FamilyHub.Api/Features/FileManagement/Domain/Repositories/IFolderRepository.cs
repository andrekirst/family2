using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFolderRepository
{
    Task<Entities.Folder?> GetByIdAsync(FolderId id, CancellationToken ct = default);
    Task<Entities.Folder?> GetRootFolderAsync(FamilyId familyId, CancellationToken ct = default);
    Task<List<Entities.Folder>> GetChildrenAsync(FolderId parentId, CancellationToken ct = default);
    Task AddAsync(Entities.Folder folder, CancellationToken ct = default);
    Task RemoveAsync(Entities.Folder folder, CancellationToken ct = default);
}
