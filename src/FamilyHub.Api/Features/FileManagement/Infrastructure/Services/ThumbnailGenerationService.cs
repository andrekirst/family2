using FamilyHub.Api.Features.FileManagement.Application.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Services;

public sealed class ThumbnailGenerationService : IThumbnailGenerationService
{
    private static readonly HashSet<string> SupportedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp", "image/tiff"
    };

    public async Task<byte[]> GenerateThumbnailAsync(
        byte[] sourceData, string mimeType, int targetWidth, int targetHeight,
        CancellationToken cancellationToken = default)
    {
        using var image = Image.Load(sourceData);

        // Auto-rotate based on EXIF orientation
        image.Mutate(x => x.AutoOrient());

        // Resize maintaining aspect ratio, fitting within target dimensions
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(targetWidth, targetHeight),
            Mode = ResizeMode.Max
        }));

        using var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream, cancellationToken);
        return outputStream.ToArray();
    }

    public bool CanGenerateThumbnail(string mimeType)
        => SupportedMimeTypes.Contains(mimeType);
}
