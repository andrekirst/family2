using FamilyHub.Api.Features.FileManagement.Application.Services;

namespace FamilyHub.TestCommon.Fakes;

public class FakeThumbnailGenerationService : IThumbnailGenerationService
{
    private static readonly HashSet<string> SupportedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp"
    };

    public Task<byte[]> GenerateThumbnailAsync(
        byte[] sourceData, string mimeType, int targetWidth, int targetHeight,
        CancellationToken ct = default)
    {
        // Return a small fake thumbnail (4 bytes representing width/height)
        var result = new byte[] { (byte)(targetWidth & 0xFF), (byte)(targetHeight & 0xFF), 0x01, 0x02 };
        return Task.FromResult(result);
    }

    public bool CanGenerateThumbnail(string mimeType)
        => SupportedMimeTypes.Contains(mimeType);
}
