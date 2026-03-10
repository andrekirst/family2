using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.UploadAvatar;

/// <summary>
/// Command to upload and process a new avatar image.
/// </summary>
public sealed record UploadAvatarCommand(
    byte[] ImageData,
    string FileName,
    string MimeType,
    float? CropX,
    float? CropY,
    float? CropWidth,
    float? CropHeight
) : ICommand<UploadAvatarResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
