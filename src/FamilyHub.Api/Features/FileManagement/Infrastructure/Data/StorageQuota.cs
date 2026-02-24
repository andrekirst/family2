namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

/// <summary>
/// Tracks storage usage and limits per family.
/// </summary>
public sealed class StorageQuota
{
    public Guid FamilyId { get; set; }
    public long UsedBytes { get; set; }
    public long MaxBytes { get; set; }
    public DateTime UpdatedAt { get; set; }
}
