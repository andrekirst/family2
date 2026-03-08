using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class FileMetadataRepository(AppDbContext context) : IFileMetadataRepository
{
    public async Task<FileMetadata?> GetByFileIdAsync(FileId fileId, CancellationToken cancellationToken = default)
        => await context.Set<FileMetadata>()
            .FirstOrDefaultAsync(m => m.FileId == fileId, cancellationToken);

    public async Task AddAsync(FileMetadata metadata, CancellationToken cancellationToken = default)
        => await context.Set<FileMetadata>().AddAsync(metadata, cancellationToken);

    public Task RemoveAsync(FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        context.Set<FileMetadata>().Remove(metadata);
        return Task.CompletedTask;
    }
}
