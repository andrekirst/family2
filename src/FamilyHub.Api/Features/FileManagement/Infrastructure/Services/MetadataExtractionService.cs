using System.Text.Json;
using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Services;

public sealed class MetadataExtractionService : IMetadataExtractionService
{
    private static readonly HashSet<string> SupportedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/tiff",
        "image/heic",
        "image/heif",
        "image/webp"
    ];

    public Task<ExtractionResult?> ExtractAsync(Stream data, string mimeType, CancellationToken ct = default)
    {
        if (!SupportedMimeTypes.Contains(mimeType.ToLowerInvariant()))
            return Task.FromResult<ExtractionResult?>(null);

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(data);

            var gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();
            var exifIfd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            var exifSubIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            Latitude? latitude = null;
            Longitude? longitude = null;

            if (gpsDirectory is not null && gpsDirectory.TryGetGeoLocation(out var geoLocation))
            {
                latitude = Latitude.From(geoLocation.Latitude);
                longitude = Longitude.From(geoLocation.Longitude);
            }

            string? cameraModel = null;
            if (exifIfd0 is not null)
            {
                var make = exifIfd0.GetDescription(ExifDirectoryBase.TagMake)?.Trim();
                var model = exifIfd0.GetDescription(ExifDirectoryBase.TagModel)?.Trim();
                cameraModel = (make, model) switch
                {
                    (not null, not null) when model.StartsWith(make) => model,
                    (not null, not null) => $"{make} {model}",
                    (null, not null) => model,
                    (not null, null) => make,
                    _ => null
                };
            }

            DateTime? captureDate = null;
            if (exifSubIfd is not null &&
                exifSubIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTime))
            {
                captureDate = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }

            // Build raw EXIF JSON from all directories
            var rawExif = new Dictionary<string, Dictionary<string, string>>();
            foreach (var directory in directories)
            {
                var tags = new Dictionary<string, string>();
                foreach (var tag in directory.Tags)
                {
                    tags[tag.Name] = tag.Description ?? string.Empty;
                }

                if (tags.Count > 0)
                    rawExif[directory.Name] = tags;
            }

            var rawExifJson = rawExif.Count > 0
                ? JsonSerializer.Serialize(rawExif)
                : null;

            var result = new ExtractionResult(latitude, longitude, cameraModel, captureDate, rawExifJson);

            // Only return result if at least some metadata was extracted
            if (latitude is null && longitude is null && cameraModel is null && captureDate is null && rawExifJson is null)
                return Task.FromResult<ExtractionResult?>(null);

            return Task.FromResult<ExtractionResult?>(result);
        }
        catch (ImageProcessingException)
        {
            return Task.FromResult<ExtractionResult?>(null);
        }
    }
}
