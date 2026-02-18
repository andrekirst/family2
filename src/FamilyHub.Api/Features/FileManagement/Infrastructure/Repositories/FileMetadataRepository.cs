using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class FileMetadataRepository(AppDbContext context) : IFileMetadataRepository
{
    public async Task<FileMetadata?> GetByFileIdAsync(FileId fileId, CancellationToken ct = default)
        => await context.Set<FileMetadata>()
            .FirstOrDefaultAsync(m => m.FileId == fileId, ct);

    public async Task AddAsync(FileMetadata metadata, CancellationToken ct = default)
        => await context.Set<FileMetadata>().AddAsync(metadata, ct);

    public Task RemoveAsync(FileMetadata metadata, CancellationToken ct = default)
    {
        context.Set<FileMetadata>().Remove(metadata);
        return Task.CompletedTask;
    }
}
