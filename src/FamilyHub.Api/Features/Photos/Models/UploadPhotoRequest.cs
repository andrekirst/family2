namespace FamilyHub.Api.Features.Photos.Models;

public class UploadPhotoRequest
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public required string StoragePath { get; set; }
    public string? Caption { get; set; }
}
