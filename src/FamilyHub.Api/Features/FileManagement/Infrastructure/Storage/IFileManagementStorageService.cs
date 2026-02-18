using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

/// <summary>
/// High-level file storage service for the FileManagement module.
/// Wraps IStorageProvider with MIME detection, checksums, and quota enforcement.
/// </summary>
public interface IFileManagementStorageService
{
    /// <summary>
    /// Stores a file with automatic MIME detection, checksum calculation, and quota enforcement.
    /// </summary>
    Task<FileStorageResult> StoreFileAsync(
        FamilyId familyId,
        Stream data,
        string fileName,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a file as a stream.
    /// </summary>
    Task<FileDownloadResult?> GetFileAsync(string storageKey, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a byte range from a file (for media streaming).
    /// </summary>
    Task<StorageRangeResult?> GetFileRangeAsync(
        string storageKey, long from, long to, CancellationToken ct = default);

    /// <summary>
    /// Deletes a file and decrements quota usage.
    /// </summary>
    Task DeleteFileAsync(
        FamilyId familyId, string storageKey, long fileSize, CancellationToken ct = default);

    /// <summary>
    /// Initiates a chunked upload session.
    /// </summary>
    Task<string> InitiateChunkedUploadAsync(CancellationToken ct = default);

    /// <summary>
    /// Uploads a single chunk for a chunked upload session.
    /// </summary>
    Task UploadChunkAsync(
        string uploadId, int chunkIndex, Stream data, CancellationToken ct = default);

    /// <summary>
    /// Completes a chunked upload by assembling all chunks.
    /// </summary>
    Task<FileStorageResult> CompleteChunkedUploadAsync(
        FamilyId familyId, string uploadId, string fileName, CancellationToken ct = default);
}

/// <summary>
/// Result of a file storage operation.
/// </summary>
public sealed record FileStorageResult(
    string StorageKey,
    MimeType DetectedMimeType,
    FileSize Size,
    Checksum Sha256Checksum);

/// <summary>
/// Result of a file download operation.
/// </summary>
public sealed record FileDownloadResult(
    Stream Data,
    string MimeType,
    long Size);
