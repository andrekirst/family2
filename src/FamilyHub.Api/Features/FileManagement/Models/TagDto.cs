namespace FamilyHub.Api.Features.FileManagement.Models;

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public int FileCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
