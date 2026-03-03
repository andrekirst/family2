namespace FamilyHub.Api.Features.Photos.Models;

public class PhotosPageDto
{
    public List<PhotoDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public bool HasMore { get; set; }
}
