namespace FamilyHub.Api.Features.FileManagement.Models;

public class AlbumDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CoverFileId { get; set; }
    public Guid FamilyId { get; set; }
    public Guid CreatedBy { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
