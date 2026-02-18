using FamilyHub.Api.Common.Infrastructure.Avatar;

namespace FamilyHub.TestCommon.Fakes;

public class FakeAvatarProcessingService : IAvatarProcessingService
{
    public bool ShouldFailValidation { get; set; }
    public string? ValidationErrorMessage { get; set; }

    public Task ValidateImageAsync(Stream imageStream, string mimeType, CancellationToken ct = default)
    {
        if (ShouldFailValidation)
        {
            throw new InvalidOperationException(ValidationErrorMessage ?? "Invalid image");
        }
        return Task.CompletedTask;
    }

    public Task<Dictionary<AvatarSize, byte[]>> ProcessAvatarAsync(
        Stream imageStream,
        CropArea? cropArea = null,
        CancellationToken ct = default)
    {
        // Return fake processed bytes for each size variant
        var variants = new Dictionary<AvatarSize, byte[]>
        {
            [AvatarSize.Tiny] = new byte[] { 1, 2, 3 },
            [AvatarSize.Small] = new byte[] { 4, 5, 6 },
            [AvatarSize.Medium] = new byte[] { 7, 8, 9 },
            [AvatarSize.Large] = new byte[] { 10, 11, 12 }
        };

        return Task.FromResult(variants);
    }
}
