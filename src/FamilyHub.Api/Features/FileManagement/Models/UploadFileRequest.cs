namespace FamilyHub.Api.Features.FileManagement.Models;

public class UploadFileRequest
{
    public required string Name { get; set; }
    public required string MimeType { get; set; }
    public required long Size { get; set; }
    public required string StorageKey { get; set; }
    public required string Checksum { get; set; }
    public required Guid FolderId { get; set; }
}
