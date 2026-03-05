namespace FamilyHub.Api.Features.Messaging.Models;

public class AttachmentDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? StorageKey { get; set; }
    public DateTime AttachedAt { get; set; }
}
