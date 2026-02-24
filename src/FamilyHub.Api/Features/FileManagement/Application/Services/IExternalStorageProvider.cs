namespace FamilyHub.Api.Features.FileManagement.Application.Services;

/// <summary>
/// Common interface for all external storage providers (OneDrive, Google Drive, Dropbox, Paperless-ngx).
/// Each provider implements this interface to enable unified file operations.
/// </summary>
public interface IExternalStorageProvider
{
    string ProviderName { get; }
    Task<List<ExternalFileInfo>> ListAsync(string path, CancellationToken ct = default);
    Task<Stream?> DownloadAsync(string fileId, CancellationToken ct = default);
    Task<string> UploadAsync(Stream data, string path, string fileName, CancellationToken ct = default);
    Task DeleteAsync(string fileId, CancellationToken ct = default);
    Task<ExternalFileInfo?> GetMetadataAsync(string fileId, CancellationToken ct = default);
    Task<ExternalStorageQuota> GetQuotaAsync(CancellationToken ct = default);
}

public sealed record ExternalFileInfo(
    string Id,
    string Name,
    string Path,
    long Size,
    string MimeType,
    bool IsFolder,
    DateTime ModifiedAt);

public sealed record ExternalStorageQuota(
    long UsedBytes,
    long TotalBytes);
