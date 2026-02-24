namespace FamilyHub.Api.Features.FileManagement.Models;

public class MediaStreamInfoDto
{
    public Guid FileId { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public bool SupportsRangeRequests { get; set; }
    public bool IsStreamable { get; set; }
    public List<FileThumbnailDto> Thumbnails { get; set; } = [];
}
