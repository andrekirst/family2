namespace FamilyHub.Api.Features.FileManagement.Models;

public class RenameFolderRequest
{
    public required Guid FolderId { get; set; }
    public required string NewName { get; set; }
}
