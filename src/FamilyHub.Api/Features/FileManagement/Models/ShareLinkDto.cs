namespace FamilyHub.Api.Features.FileManagement.Models;

public class ShareLinkDto
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public Guid FamilyId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool HasPassword { get; set; }
    public int? MaxDownloads { get; set; }
    public int DownloadCount { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsExpired { get; set; }
    public bool IsAccessible { get; set; }
    public DateTime CreatedAt { get; set; }
}
