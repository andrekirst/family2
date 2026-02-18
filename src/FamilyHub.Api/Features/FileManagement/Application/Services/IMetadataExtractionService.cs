using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Services;

/// <summary>
/// Extracts EXIF metadata from image files (GPS coordinates, camera info, capture date).
/// </summary>
public interface IMetadataExtractionService
{
    /// <summary>
    /// Extracts metadata from the given image stream.
    /// Returns null if the file is not a supported image format or has no extractable metadata.
    /// </summary>
    Task<ExtractionResult?> ExtractAsync(Stream data, string mimeType, CancellationToken ct = default);
}

public sealed record ExtractionResult(
    Latitude? GpsLatitude,
    Longitude? GpsLongitude,
    string? CameraModel,
    DateTime? CaptureDate,
    string? RawExifJson);
