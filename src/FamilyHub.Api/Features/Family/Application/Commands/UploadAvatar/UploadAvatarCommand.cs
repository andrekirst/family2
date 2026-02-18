using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.UploadAvatar;

/// <summary>
/// Command to upload and process a new avatar image.
/// </summary>
public sealed record UploadAvatarCommand(
    UserId UserId,
    byte[] ImageData,
    string FileName,
    string MimeType,
    float? CropX,
    float? CropY,
    float? CropWidth,
    float? CropHeight
) : ICommand<UploadAvatarResult>;
