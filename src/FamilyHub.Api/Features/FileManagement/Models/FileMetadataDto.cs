namespace FamilyHub.Api.Features.FileManagement.Models;

public class FileMetadataDto
{
    public Guid FileId { get; set; }
    public double? GpsLatitude { get; set; }
    public double? GpsLongitude { get; set; }
    public string? LocationName { get; set; }
    public string? CameraModel { get; set; }
    public DateTime? CaptureDate { get; set; }
    public bool HasGpsData { get; set; }
}
