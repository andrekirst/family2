using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFileThumbnailRepository : IFileThumbnailRepository
{
    public List<FileThumbnail> Thumbnails { get; } = [];

    public Task<List<FileThumbnail>> GetByFileIdAsync(FileId fileId, CancellationToken ct = default)
        => Task.FromResult(Thumbnails
            .Where(t => t.FileId == fileId)
            .OrderBy(t => t.Width)
            .ToList());

    public Task<FileThumbnail?> GetByFileIdAndSizeAsync(FileId fileId, int width, int height, CancellationToken ct = default)
        => Task.FromResult(Thumbnails.FirstOrDefault(t => t.FileId == fileId && t.Width == width && t.Height == height));

    public Task AddAsync(FileThumbnail thumbnail, CancellationToken ct = default)
    {
        Thumbnails.Add(thumbnail);
        return Task.CompletedTask;
    }

    public Task RemoveByFileIdAsync(FileId fileId, CancellationToken ct = default)
    {
        Thumbnails.RemoveAll(t => t.FileId == fileId);
        return Task.CompletedTask;
    }
}
