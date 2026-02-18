namespace FamilyHub.Api.Features.FileManagement.Models;

public class ZipJobDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public Guid InitiatedBy { get; set; }
    public int FileCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public long? ZipSize { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
