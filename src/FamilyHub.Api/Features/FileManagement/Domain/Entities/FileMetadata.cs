using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

public sealed class FileMetadata
{
#pragma warning disable CS8618
    private FileMetadata() { }
#pragma warning restore CS8618

    public static FileMetadata Create(
        FileId fileId,
        Latitude? gpsLatitude,
        Longitude? gpsLongitude,
        string? locationName,
        string? cameraModel,
        DateTime? captureDate,
        string? rawExif)
    {
        return new FileMetadata
        {
            FileId = fileId,
            GpsLatitude = gpsLatitude,
            GpsLongitude = gpsLongitude,
            LocationName = locationName,
            CameraModel = cameraModel,
            CaptureDate = captureDate,
            RawExif = rawExif,
            ExtractedAt = DateTime.UtcNow
        };
    }

    public FileId FileId { get; private set; }
    public Latitude? GpsLatitude { get; private set; }
    public Longitude? GpsLongitude { get; private set; }
    public string? LocationName { get; private set; }
    public string? CameraModel { get; private set; }
    public DateTime? CaptureDate { get; private set; }
    public string? RawExif { get; private set; }
    public DateTime ExtractedAt { get; private set; }

    public bool HasGpsData => GpsLatitude.HasValue && GpsLongitude.HasValue;

    public void UpdateLocationName(string locationName)
    {
        LocationName = locationName;
    }
}
