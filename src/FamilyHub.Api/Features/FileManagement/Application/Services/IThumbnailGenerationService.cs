namespace FamilyHub.Api.Features.FileManagement.Application.Services;

/// <summary>
/// Service for generating image thumbnails from file data.
/// Supports image resizing via ImageSharp.
/// </summary>
public interface IThumbnailGenerationService
{
    Task<byte[]> GenerateThumbnailAsync(byte[] sourceData, string mimeType, int targetWidth, int targetHeight, CancellationToken cancellationToken = default);
    bool CanGenerateThumbnail(string mimeType);
}
