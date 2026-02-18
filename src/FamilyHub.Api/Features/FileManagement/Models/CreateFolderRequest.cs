namespace FamilyHub.Api.Features.FileManagement.Models;

public class CreateFolderRequest
{
    public required string Name { get; set; }
    public Guid? ParentFolderId { get; set; }
}
