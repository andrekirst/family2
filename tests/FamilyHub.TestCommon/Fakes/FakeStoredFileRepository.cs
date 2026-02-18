using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeStoredFileRepository : IStoredFileRepository
{
    public List<StoredFile> Files { get; } = [];

    public Task<StoredFile?> GetByIdAsync(FileId id, CancellationToken ct = default)
        => Task.FromResult(Files.FirstOrDefault(f => f.Id == id));

    public Task<List<StoredFile>> GetByFolderIdAsync(FolderId folderId, CancellationToken ct = default)
        => Task.FromResult(Files.Where(f => f.FolderId == folderId).ToList());

    public Task<List<StoredFile>> GetByFolderIdsAsync(IEnumerable<FolderId> folderIds, CancellationToken ct = default)
    {
        var ids = folderIds.ToList();
        return Task.FromResult(Files.Where(f => ids.Contains(f.FolderId)).ToList());
    }

    public Task AddAsync(StoredFile file, CancellationToken ct = default)
    {
        Files.Add(file);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(StoredFile file, CancellationToken ct = default)
    {
        Files.Remove(file);
        return Task.CompletedTask;
    }

    public Task RemoveRangeAsync(IEnumerable<StoredFile> files, CancellationToken ct = default)
    {
        foreach (var file in files.ToList())
            Files.Remove(file);
        return Task.CompletedTask;
    }
}
