using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class FileVersionRepository(AppDbContext context) : IFileVersionRepository
{
    public async Task<FileVersion?> GetByIdAsync(FileVersionId id, CancellationToken ct = default)
        => await context.Set<FileVersion>().FindAsync([id], cancellationToken: ct);

    public async Task<List<FileVersion>> GetByFileIdAsync(FileId fileId, CancellationToken ct = default)
        => await context.Set<FileVersion>()
            .Where(v => v.FileId == fileId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(ct);

    public async Task<FileVersion?> GetCurrentVersionAsync(FileId fileId, CancellationToken ct = default)
        => await context.Set<FileVersion>()
            .FirstOrDefaultAsync(v => v.FileId == fileId && v.IsCurrent, ct);

    public async Task<int> GetMaxVersionNumberAsync(FileId fileId, CancellationToken ct = default)
    {
        var versions = await context.Set<FileVersion>()
            .Where(v => v.FileId == fileId)
            .Select(v => v.VersionNumber)
            .ToListAsync(ct);

        return versions.Count > 0 ? versions.Max() : 0;
    }

    public async Task AddAsync(FileVersion version, CancellationToken ct = default)
        => await context.Set<FileVersion>().AddAsync(version, ct);
}
