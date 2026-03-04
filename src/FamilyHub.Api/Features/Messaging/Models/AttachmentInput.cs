namespace FamilyHub.Api.Features.Messaging.Models;

/// <summary>
/// GraphQL input for a file attachment. Uses primitives (ADR-003).
/// The client sends metadata obtained from the /api/files/upload response.
/// </summary>
public class AttachmentInput
{
    public string StorageKey { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Checksum { get; set; } = string.Empty;
}
