namespace FamilyHub.Api.Features.FileManagement.Models;

public class FolderDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public string MaterializedPath { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
