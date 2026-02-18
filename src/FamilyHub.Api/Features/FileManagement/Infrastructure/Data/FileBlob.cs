namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

/// <summary>
/// EF Core entity for binary file storage in the file_management schema.
/// Stores raw binary data alongside minimal metadata for the storage layer.
/// </summary>
public sealed class FileBlob
{
    public string StorageKey { get; set; } = string.Empty;
    public byte[] Data { get; set; } = [];
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
}
