namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

/// <summary>
/// Pluggable backend for binary file storage.
/// V1: PostgreSQL bytea. Future: S3, MinIO, Azure Blob Storage.
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// Stores binary data and returns a unique storage key.
    /// </summary>
    Task<string> UploadAsync(Stream data, string mimeType, CancellationToken ct = default);

    /// <summary>
    /// Retrieves binary data as a stream. Returns null if not found.
    /// </summary>
    Task<Stream?> DownloadAsync(string storageKey, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a byte range from the stored file. Used for media streaming.
    /// Returns null if not found.
    /// </summary>
    Task<StorageRangeResult?> DownloadRangeAsync(string storageKey, long from, long to, CancellationToken ct = default);

    /// <summary>
    /// Deletes the stored binary data.
    /// </summary>
    Task DeleteAsync(string storageKey, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a storage key exists.
    /// </summary>
    Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default);

    /// <summary>
    /// Returns the size in bytes of the stored file. Returns null if not found.
    /// </summary>
    Task<long?> GetSizeAsync(string storageKey, CancellationToken ct = default);
}

/// <summary>
/// Result of a range-based download (for HTTP range requests / media streaming).
/// </summary>
public sealed record StorageRangeResult(
    Stream Data,
    long RangeStart,
    long RangeEnd,
    long TotalSize);
