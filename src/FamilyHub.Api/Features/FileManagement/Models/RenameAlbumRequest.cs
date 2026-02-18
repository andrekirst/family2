namespace FamilyHub.Api.Features.FileManagement.Models;

public class RenameAlbumRequest
{
    public Guid AlbumId { get; set; }
    public string Name { get; set; } = string.Empty;
}
