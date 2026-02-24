using FamilyHub.Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

/// <summary>
/// PostgreSQL-backed storage provider using bytea columns.
/// Stores binary data in the file_management.file_blobs table.
/// Supports files up to 100MB.
/// </summary>
public sealed class PostgresStorageProvider(AppDbContext dbContext) : IStorageProvider
{
    public async Task<string> UploadAsync(Stream data, string mimeType, CancellationToken ct = default)
    {
        var storageKey = Guid.NewGuid().ToString();

        using var memoryStream = new MemoryStream();
        await data.CopyToAsync(memoryStream, ct);
        var bytes = memoryStream.ToArray();

        var blob = new Data.FileBlob
        {
            StorageKey = storageKey,
            Data = bytes,
            MimeType = mimeType,
            Size = bytes.Length,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Set<Data.FileBlob>().Add(blob);
        await dbContext.SaveChangesAsync(ct);

        return storageKey;
    }

    public async Task<Stream?> DownloadAsync(string storageKey, CancellationToken ct = default)
    {
        var blob = await dbContext.Set<Data.FileBlob>()
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.StorageKey == storageKey, ct);

        return blob?.Data is not null ? new MemoryStream(blob.Data) : null;
    }

    public async Task<StorageRangeResult?> DownloadRangeAsync(
        string storageKey, long from, long to, CancellationToken ct = default)
    {
        var blob = await dbContext.Set<Data.FileBlob>()
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.StorageKey == storageKey, ct);

        if (blob?.Data is null)
            return null;

        var totalSize = blob.Data.Length;
        var rangeEnd = Math.Min(to, totalSize - 1);
        var length = (int)(rangeEnd - from + 1);

        var rangeData = new byte[length];
        Array.Copy(blob.Data, from, rangeData, 0, length);

        return new StorageRangeResult(
            new MemoryStream(rangeData),
            from,
            rangeEnd,
            totalSize);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var blob = await dbContext.Set<Data.FileBlob>()
            .FirstOrDefaultAsync(f => f.StorageKey == storageKey, ct);

        if (blob is not null)
        {
            dbContext.Set<Data.FileBlob>().Remove(blob);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default)
    {
        return await dbContext.Set<Data.FileBlob>()
            .AsNoTracking()
            .AnyAsync(f => f.StorageKey == storageKey, ct);
    }

    public async Task<long?> GetSizeAsync(string storageKey, CancellationToken ct = default)
    {
        return await dbContext.Set<Data.FileBlob>()
            .AsNoTracking()
            .Where(f => f.StorageKey == storageKey)
            .Select(f => (long?)f.Size)
            .FirstOrDefaultAsync(ct);
    }
}
