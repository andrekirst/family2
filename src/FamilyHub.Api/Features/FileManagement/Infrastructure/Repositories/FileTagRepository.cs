using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class FileTagRepository(AppDbContext context) : IFileTagRepository
{
    public async Task<List<FileTag>> GetByFileIdAsync(FileId fileId, CancellationToken ct = default)
        => await context.Set<FileTag>()
            .Where(ft => ft.FileId == fileId)
            .ToListAsync(ct);

    public async Task<List<FileTag>> GetByTagIdAsync(TagId tagId, CancellationToken ct = default)
        => await context.Set<FileTag>()
            .Where(ft => ft.TagId == tagId)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(FileId fileId, TagId tagId, CancellationToken ct = default)
        => await context.Set<FileTag>()
            .AnyAsync(ft => ft.FileId == fileId && ft.TagId == tagId, ct);

    public async Task AddAsync(FileTag fileTag, CancellationToken ct = default)
        => await context.Set<FileTag>().AddAsync(fileTag, ct);

    public async Task AddRangeAsync(IEnumerable<FileTag> fileTags, CancellationToken ct = default)
        => await context.Set<FileTag>().AddRangeAsync(fileTags, ct);

    public Task RemoveAsync(FileTag fileTag, CancellationToken ct = default)
    {
        context.Set<FileTag>().Remove(fileTag);
        return Task.CompletedTask;
    }

    public async Task RemoveByTagIdAsync(TagId tagId, CancellationToken ct = default)
    {
        var fileTags = await context.Set<FileTag>()
            .Where(ft => ft.TagId == tagId)
            .ToListAsync(ct);
        context.Set<FileTag>().RemoveRange(fileTags);
    }

    public async Task<int> GetFileCountByTagIdAsync(TagId tagId, CancellationToken ct = default)
        => await context.Set<FileTag>()
            .CountAsync(ft => ft.TagId == tagId, ct);

    public async Task<List<FileId>> GetFileIdsByTagIdsAsync(IEnumerable<TagId> tagIds, CancellationToken ct = default)
    {
        var tagIdList = tagIds.ToList();
        var tagCount = tagIdList.Count;

        // AND logic: file must have ALL specified tags
        return await context.Set<FileTag>()
            .Where(ft => tagIdList.Contains(ft.TagId))
            .GroupBy(ft => ft.FileId)
            .Where(g => g.Count() == tagCount)
            .Select(g => g.Key)
            .ToListAsync(ct);
    }
}
