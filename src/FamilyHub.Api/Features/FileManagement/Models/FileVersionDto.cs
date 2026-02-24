namespace FamilyHub.Api.Features.FileManagement.Models;

public class FileVersionDto
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public int VersionNumber { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public Guid UploadedBy { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime UploadedAt { get; set; }
}
