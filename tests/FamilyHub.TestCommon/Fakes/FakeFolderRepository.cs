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
}
