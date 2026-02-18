namespace FamilyHub.Api.Features.FileManagement.Models;

public class CreateAlbumRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
