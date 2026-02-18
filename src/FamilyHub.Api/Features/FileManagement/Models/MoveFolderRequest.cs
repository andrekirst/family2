namespace FamilyHub.Api.Features.FileManagement.Models;

public class MoveFolderRequest
{
    public required Guid FolderId { get; set; }
    public required Guid TargetParentFolderId { get; set; }
}
