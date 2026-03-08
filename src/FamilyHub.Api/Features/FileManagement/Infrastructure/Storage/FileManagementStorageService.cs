using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Data;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

/// <summary>
/// High-level file storage service combining the storage provider with
/// MIME detection, checksum calculation, and quota enforcement.
/// </summary>
public sealed class FileManagementStorageService(
    IStorageProvider storageProvider,
    IMimeDetector mimeDetector,
    IChecksumCalculator checksumCalculator,
    IStorageQuotaService quotaService,
    AppDbContext dbContext,
    IOptions<StorageQuotaOptions> options) : IFileManagementStorageService
{
    private readonly StorageQuotaOptions _options = options.Value;

    public async Task<FileStorageResult> StoreFileAsync(
        FamilyId familyId, Stream data, string fileName, CancellationToken cancellationToken = default)
    {
        // Read stream into memory for processing
        using var memoryStream = new MemoryStream();
        await data.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        // Validate file size
        if (bytes.Length > _options.MaxFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"File exceeds maximum size of {_options.MaxFileSizeBytes / (1024 * 1024)} MB");
        }

        // Check quota
        if (!await quotaService.CanUploadAsync(familyId, bytes.Length, cancellationToken))
        {
            throw new InvalidOperationException("Storage quota exceeded");
        }

        // Detect MIME type from content (first 512 bytes)
        var headerLength = Math.Min(bytes.Length, 512);
        var detectedMime = mimeDetector.Detect(bytes.AsSpan(0, headerLength), fileName);
        var mimeType = MimeType.From(detectedMime);

        // Calculate checksum
        var checksumHex = checksumCalculator.Compute(bytes);
        var checksum = Checksum.From(checksumHex);

        // Store via provider
        memoryStream.Position = 0;
        var storageKey = await storageProvider.UploadAsync(memoryStream, detectedMime, cancellationToken);

        // Update quota
        await quotaService.IncrementUsageAsync(familyId, bytes.Length, cancellationToken);

        return new FileStorageResult(
            storageKey,
            mimeType,
            FileSize.From(bytes.Length),
            checksum);
    }

    public async Task<FileDownloadResult?> GetFileAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var stream = await storageProvider.DownloadAsync(storageKey, cancellationToken);
        if (stream is null)
        {
            return null;
        }

        var size = await storageProvider.GetSizeAsync(storageKey, cancellationToken);

        // Get MIME type from blob metadata
        var blob = await dbContext.Set<FileBlob>()
            .AsNoTracking()
            .Where(f => f.StorageKey == storageKey)
            .Select(f => new { f.MimeType })
            .FirstOrDefaultAsync(cancellationToken);

        return new FileDownloadResult(
            stream,
            blob?.MimeType ?? "application/octet-stream",
            size ?? 0);
    }

    public Task<StorageRangeResult?> GetFileRangeAsync(
        string storageKey, long from, long to, CancellationToken cancellationToken = default)
    {
        return storageProvider.DownloadRangeAsync(storageKey, from, to, cancellationToken);
    }

    public async Task DeleteFileAsync(
        FamilyId familyId, string storageKey, long fileSize, CancellationToken cancellationToken = default)
    {
        await storageProvider.DeleteAsync(storageKey, cancellationToken);
        await quotaService.DecrementUsageAsync(familyId, fileSize, cancellationToken);
    }

    public Task<string> InitiateChunkedUploadAsync(CancellationToken cancellationToken = default)
    {
        var uploadId = Guid.NewGuid().ToString();
        return Task.FromResult(uploadId);
    }

    public async Task UploadChunkAsync(
        string uploadId, int chunkIndex, Stream data, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await data.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        var chunk = new UploadChunk
        {
            Id = Guid.NewGuid(),
            UploadId = uploadId,
            ChunkIndex = chunkIndex,
            Data = bytes,
            Size = bytes.Length,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Set<UploadChunk>().Add(chunk);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<FileStorageResult> CompleteChunkedUploadAsync(
        FamilyId familyId, string uploadId, string fileName, CancellationToken cancellationToken = default)
    {
        // Retrieve all chunks in order
        var chunks = await dbContext.Set<UploadChunk>()
            .Where(c => c.UploadId == uploadId)
            .OrderBy(c => c.ChunkIndex)
            .ToListAsync(cancellationToken);

        if (chunks.Count == 0)
        {
            throw new InvalidOperationException("No chunks found for upload ID");
        }

        // Assemble chunks into a single stream
        using var assembledStream = new MemoryStream();
        foreach (var chunk in chunks)
        {
            await assembledStream.WriteAsync(chunk.Data, cancellationToken);
        }
        assembledStream.Position = 0;

        // Store the assembled file
        var result = await StoreFileAsync(familyId, assembledStream, fileName, cancellationToken);

        // Clean up chunks
        dbContext.Set<UploadChunk>().RemoveRange(chunks);
        await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }
}
