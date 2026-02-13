using FamilyHub.Common.Domain;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// Image processing service using SixLabors.ImageSharp.
/// Validates images, applies crops, and generates all size variants.
/// </summary>
public sealed class AvatarProcessingService : IAvatarProcessingService
{
    private static readonly HashSet<string> AllowedMimeTypes =
        ["image/jpeg", "image/png", "image/webp"];

    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private const int MaxDimension = 4096;

    public async Task ValidateImageAsync(Stream imageStream, string mimeType, CancellationToken ct = default)
    {
        if (!AllowedMimeTypes.Contains(mimeType.ToLowerInvariant()))
        {
            throw new DomainException($"Unsupported image type '{mimeType}'. Allowed: JPEG, PNG, WebP.");
        }

        if (imageStream.Length > MaxFileSizeBytes)
        {
            throw new DomainException($"Image exceeds maximum size of 5 MB.");
        }

        // Content validation: try to actually load the image with ImageSharp
        imageStream.Position = 0;
        try
        {
            using var image = await Image.LoadAsync(imageStream, ct);
            if (image.Width > MaxDimension || image.Height > MaxDimension)
            {
                throw new DomainException($"Image dimensions exceed maximum of {MaxDimension}x{MaxDimension}.");
            }
        }
        catch (UnknownImageFormatException)
        {
            throw new DomainException("File is not a valid image.");
        }

        imageStream.Position = 0;
    }

    public async Task<Dictionary<AvatarSize, byte[]>> ProcessAvatarAsync(
        Stream imageStream,
        CropArea? cropArea = null,
        CancellationToken ct = default)
    {
        imageStream.Position = 0;
        using var image = await Image.LoadAsync(imageStream, ct);

        // Apply crop if provided
        if (cropArea is not null)
        {
            var cropRect = new Rectangle(
                (int)(cropArea.X * image.Width),
                (int)(cropArea.Y * image.Height),
                (int)(cropArea.Width * image.Width),
                (int)(cropArea.Height * image.Height));

            image.Mutate(x => x.Crop(cropRect));
        }

        // Generate all size variants
        var variants = new Dictionary<AvatarSize, byte[]>();
        foreach (var size in Enum.GetValues<AvatarSize>())
        {
            var pixels = (int)size;
            using var variant = image.Clone(x => x.Resize(pixels, pixels));

            using var ms = new MemoryStream();
            await variant.SaveAsync(ms, new JpegEncoder { Quality = 85 }, ct);
            variants[size] = ms.ToArray();
        }

        return variants;
    }
}
