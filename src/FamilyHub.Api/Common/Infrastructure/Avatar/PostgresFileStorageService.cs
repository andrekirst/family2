using FamilyHub.Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// PostgreSQL-backed file storage using a simple table with bytea columns.
/// Phase 1 implementation - can be swapped for S3/Azure Blob later.
/// </summary>
public sealed class PostgresFileStorageService(AppDbContext dbContext, TimeProvider timeProvider) : IFileStorageService
{
    public async Task<string> SaveAsync(byte[] data, string mimeType, CancellationToken cancellationToken = default)
    {
        var storageKey = Guid.NewGuid().ToString();
        var fileRecord = new StoredFile
        {
            StorageKey = storageKey,
            Data = data,
            MimeType = mimeType,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };

        dbContext.StoredFiles.Add(fileRecord);
        await dbContext.SaveChangesAsync(cancellationToken);

        return storageKey;
    }

    public async Task<byte[]?> GetAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var file = await dbContext.StoredFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.StorageKey == storageKey, cancellationToken);

        return file?.Data;
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var file = await dbContext.StoredFiles
            .FirstOrDefaultAsync(f => f.StorageKey == storageKey, cancellationToken);

        if (file is not null)
        {
            dbContext.StoredFiles.Remove(file);
        }
    }
}
