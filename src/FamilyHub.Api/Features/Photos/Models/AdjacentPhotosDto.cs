namespace FamilyHub.Api.Features.Photos.Models;

public class AdjacentPhotosDto
{
    public PhotoDto? Previous { get; set; }
    public PhotoDto? Next { get; set; }
}
