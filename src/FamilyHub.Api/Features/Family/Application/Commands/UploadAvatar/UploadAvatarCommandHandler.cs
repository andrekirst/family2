using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Common.Infrastructure.Avatar;
using FamilyHub.Api.Features.Auth.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Commands.UploadAvatar;

/// <summary>
/// Handler for UploadAvatarCommand.
/// Validates, processes the image into size variants, stores them, creates the Avatar aggregate,
/// and updates the User's global avatar reference.
/// </summary>
public sealed class UploadAvatarCommandHandler(
    IUserRepository userRepository,
    IAvatarRepository avatarRepository,
    IAvatarProcessingService processingService,
    IFileStorageService fileStorageService)
    : ICommandHandler<UploadAvatarCommand, UploadAvatarResult>
{
    public async ValueTask<UploadAvatarResult> Handle(
        UploadAvatarCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new DomainException("User not found");

        // Validate the image content with ImageSharp
        using var imageStream = new MemoryStream(command.ImageData);
        await processingService.ValidateImageAsync(imageStream, command.MimeType, cancellationToken);

        // Build optional crop area
        CropArea? cropArea = null;
        if (command.CropX.HasValue && command.CropY.HasValue &&
            command.CropWidth.HasValue && command.CropHeight.HasValue)
        {
            cropArea = new CropArea(
                command.CropX.Value, command.CropY.Value,
                command.CropWidth.Value, command.CropHeight.Value);
        }

        // Process image into all size variants
        var variants = await processingService.ProcessAvatarAsync(imageStream, cropArea, cancellationToken);

        // Store each variant and build metadata
        var variantData = new Dictionary<AvatarSize, AvatarVariantData>();
        foreach (var (size, bytes) in variants)
        {
            var storageKey = await fileStorageService.SaveAsync(bytes, "image/jpeg", cancellationToken);
            var pixels = (int)size;
            variantData[size] = new AvatarVariantData(storageKey, "image/jpeg", bytes.Length, pixels, pixels);
        }

        // Delete previous avatar if exists
        if (user.AvatarId.HasValue)
        {
            var previousAvatar = await avatarRepository.GetByIdAsync(user.AvatarId.Value, cancellationToken);
            if (previousAvatar is not null)
            {
                // Delete stored files for previous variants
                foreach (var variant in previousAvatar.Variants)
                {
                    await fileStorageService.DeleteAsync(variant.StorageKey, cancellationToken);
                }
                await avatarRepository.DeleteAsync(previousAvatar.Id, cancellationToken);
            }
        }

        // Create new avatar aggregate
        var avatar = AvatarAggregate.Create(command.FileName, command.MimeType, variantData);
        await avatarRepository.AddAsync(avatar, cancellationToken);

        // Update user's global avatar (raises UserAvatarChangedEvent)
        user.SetAvatar(avatar.Id);

        return new UploadAvatarResult(avatar.Id);
    }
}
