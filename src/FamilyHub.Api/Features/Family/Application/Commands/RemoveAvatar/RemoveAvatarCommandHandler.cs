using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Common.Infrastructure.Avatar;
using FamilyHub.Api.Features.Auth.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Commands.RemoveAvatar;

/// <summary>
/// Handler for RemoveAvatarCommand.
/// Deletes the avatar aggregate and stored files, then clears the User's avatar reference.
/// </summary>
public sealed class RemoveAvatarCommandHandler(
    IUserRepository userRepository,
    IAvatarRepository avatarRepository,
    IFileStorageService fileStorageService)
    : ICommandHandler<RemoveAvatarCommand, RemoveAvatarResult>
{
    public async ValueTask<RemoveAvatarResult> Handle(
        RemoveAvatarCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new DomainException("User not found");

        if (!user.AvatarId.HasValue)
        {
            return new RemoveAvatarResult(true); // No avatar to remove
        }

        var avatar = await avatarRepository.GetByIdAsync(user.AvatarId.Value, cancellationToken);
        if (avatar is not null)
        {
            // Delete stored file data for all variants
            foreach (var variant in avatar.Variants)
            {
                await fileStorageService.DeleteAsync(variant.StorageKey, cancellationToken);
            }
            await avatarRepository.DeleteAsync(avatar.Id, cancellationToken);
        }

        // Clear user's avatar reference (raises UserAvatarRemovedEvent)
        user.RemoveAvatar();

        return new RemoveAvatarResult(true);
    }
}
