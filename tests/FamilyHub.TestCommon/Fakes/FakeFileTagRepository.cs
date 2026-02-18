using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFileTagRepository : IFileTagRepository
{
    public List<FileTag> FileTags { get; } = [];

    public Task<List<FileTag>> GetByFileIdAsync(FileId fileId, CancellationToken ct = default)
        => Task.FromResult(FileTags.Where(ft => ft.FileId == fileId).ToList());

    public Task<List<FileTag>> GetByTagIdAsync(TagId tagId, CancellationToken ct = default)
        => Task.FromResult(FileTags.Where(ft => ft.TagId == tagId).ToList());

    public Task<bool> ExistsAsync(FileId fileId, TagId tagId, CancellationToken ct = default)
        => Task.FromResult(FileTags.Any(ft => ft.FileId == fileId && ft.TagId == tagId));

    public Task AddAsync(FileTag fileTag, CancellationToken ct = default)
    {
        FileTags.Add(fileTag);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<FileTag> fileTags, CancellationToken ct = default)
    {
        FileTags.AddRange(fileTags);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(FileTag fileTag, CancellationToken ct = default)
    {
        var existing = FileTags.FirstOrDefault(ft => ft.FileId == fileTag.FileId && ft.TagId == fileTag.TagId);
        if (existing is not null) FileTags.Remove(existing);
        return Task.CompletedTask;
    }

    public Task RemoveByTagIdAsync(TagId tagId, CancellationToken ct = default)
    {
        FileTags.RemoveAll(ft => ft.TagId == tagId);
        return Task.CompletedTask;
    }

    public Task<int> GetFileCountByTagIdAsync(TagId tagId, CancellationToken ct = default)
        => Task.FromResult(FileTags.Count(ft => ft.TagId == tagId));

    public Task<List<FileId>> GetFileIdsByTagIdsAsync(IEnumerable<TagId> tagIds, CancellationToken ct = default)
    {
        var tagIdList = tagIds.ToList();
        var tagCount = tagIdList.Count;

        var fileIds = FileTags
            .Where(ft => tagIdList.Contains(ft.TagId))
            .GroupBy(ft => ft.FileId)
            .Where(g => g.Count() == tagCount)
            .Select(g => g.Key)
            .ToList();

        return Task.FromResult(fileIds);
    }
}
