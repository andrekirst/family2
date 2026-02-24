namespace FamilyHub.Api.Features.FileManagement.Models;

public class StoredFileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;
    public Guid FolderId { get; set; }
    public Guid FamilyId { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
