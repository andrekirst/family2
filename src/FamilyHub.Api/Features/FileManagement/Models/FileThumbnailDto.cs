namespace FamilyHub.Api.Features.FileManagement.Models;

public class FileThumbnailDto
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
