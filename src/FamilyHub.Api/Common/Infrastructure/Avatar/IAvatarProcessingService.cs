namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// Image processing service for avatar uploads.
/// Validates, crops, and generates size variants using SixLabors.ImageSharp.
/// </summary>
public interface IAvatarProcessingService
{
    /// <summary>
    /// Validates the uploaded image (MIME type, dimensions, content integrity).
    /// Throws if validation fails.
    /// </summary>
    Task ValidateImageAsync(Stream imageStream, string mimeType, CancellationToken ct = default);

    /// <summary>
    /// Processes an uploaded image: crops to square (if crop area provided)
    /// and generates all size variants.
    /// Returns a dictionary of AvatarSize -> processed JPEG bytes.
    /// </summary>
    Task<Dictionary<AvatarSize, byte[]>> ProcessAvatarAsync(
        Stream imageStream,
        CropArea? cropArea = null,
        CancellationToken ct = default);
}

/// <summary>
/// Defines the crop area within the original image (normalized 0-1 coordinates).
/// </summary>
public record CropArea(float X, float Y, float Width, float Height);
