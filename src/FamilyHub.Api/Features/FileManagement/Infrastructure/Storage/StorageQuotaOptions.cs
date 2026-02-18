namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

/// <summary>
/// Configuration for file storage quotas, bound from appsettings.json.
/// Section: "FileManagement:Storage"
/// </summary>
public sealed class StorageQuotaOptions
{
    public const string SectionName = "FileManagement:Storage";

    /// <summary>
    /// Default storage quota per family in bytes. Default: 5 GB.
    /// </summary>
    public long DefaultQuotaBytes { get; set; } = 5L * 1024 * 1024 * 1024;

    /// <summary>
    /// Maximum single file size in bytes. Default: 100 MB.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 100L * 1024 * 1024;

    /// <summary>
    /// Chunk size for chunked uploads in bytes. Default: 5 MB.
    /// </summary>
    public long ChunkSizeBytes { get; set; } = 5L * 1024 * 1024;
}
