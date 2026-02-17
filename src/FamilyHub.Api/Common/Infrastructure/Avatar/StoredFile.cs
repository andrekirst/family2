namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// EF Core entity for binary file storage.
/// Uses bytea column in PostgreSQL. Simple key-value store for file data.
/// </summary>
public sealed class StoredFile
{
    public string StorageKey { get; set; } = string.Empty;
    public byte[] Data { get; set; } = [];
    public string MimeType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
