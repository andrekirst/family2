namespace FamilyHub.Api.Features.FileManagement.Models;

public class RenameFileRequest
{
    public required Guid FileId { get; set; }
    public required string NewName { get; set; }
}
