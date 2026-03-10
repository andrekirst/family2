using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class FileVersionRepository(AppDbContext context) : IFileVersionRepository
{
    public async Task<FileVersion?> GetByIdAsync(FileVersionId id, CancellationToken cancellationToken = default)
        => await context.Set<FileVersion>().FindAsync([id], cancellationToken: cancellationToken);

    public async Task<bool> ExistsByIdAsync(FileVersionId id, CancellationToken cancellationToken = default)
        => await context.Set<FileVersion>().AnyAsync(v => v.Id == id, cancellationToken);

    public async Task<List<FileVersion>> GetByFileIdAsync(FileId fileId, CancellationToken cancellationToken = default)
        => await context.Set<FileVersion>()
            .Where(v => v.FileId == fileId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);

    public async Task<FileVersion?> GetCurrentVersionAsync(FileId fileId, CancellationToken cancellationToken = default)
        => await context.Set<FileVersion>()
            .FirstOrDefaultAsync(v => v.FileId == fileId && v.IsCurrent, cancellationToken);

    public async Task<int> GetMaxVersionNumberAsync(FileId fileId, CancellationToken cancellationToken = default)
    {
        var versions = await context.Set<FileVersion>()
            .Where(v => v.FileId == fileId)
            .Select(v => v.VersionNumber)
            .ToListAsync(cancellationToken);

        return versions.Count > 0 ? versions.Max() : 0;
    }

    public async Task AddAsync(FileVersion version, CancellationToken cancellationToken = default)
        => await context.Set<FileVersion>().AddAsync(version, cancellationToken);
}
