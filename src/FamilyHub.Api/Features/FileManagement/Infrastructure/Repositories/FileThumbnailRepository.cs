using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class FileThumbnailRepository(AppDbContext context) : IFileThumbnailRepository
{
    public async Task<List<FileThumbnail>> GetByFileIdAsync(FileId fileId, CancellationToken ct = default)
        => await context.Set<FileThumbnail>()
            .Where(t => t.FileId == fileId)
            .OrderBy(t => t.Width)
            .ToListAsync(ct);

    public async Task<FileThumbnail?> GetByFileIdAndSizeAsync(FileId fileId, int width, int height, CancellationToken ct = default)
        => await context.Set<FileThumbnail>()
            .FirstOrDefaultAsync(t => t.FileId == fileId && t.Width == width && t.Height == height, ct);

    public async Task AddAsync(FileThumbnail thumbnail, CancellationToken ct = default)
        => await context.Set<FileThumbnail>().AddAsync(thumbnail, ct);

    public async Task RemoveByFileIdAsync(FileId fileId, CancellationToken ct = default)
    {
        var thumbnails = await context.Set<FileThumbnail>()
            .Where(t => t.FileId == fileId)
            .ToListAsync(ct);
        context.Set<FileThumbnail>().RemoveRange(thumbnails);
    }
}
