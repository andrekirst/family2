namespace FamilyHub.Api.Features.FileManagement.Models;

public class MoveFileRequest
{
    public required Guid FileId { get; set; }
    public required Guid TargetFolderId { get; set; }
}
