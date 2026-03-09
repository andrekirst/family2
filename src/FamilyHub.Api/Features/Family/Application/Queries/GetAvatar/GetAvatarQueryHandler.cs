using FamilyHub.Api.Features.Family.Infrastructure.Avatar;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetAvatar;

public sealed class GetAvatarQueryHandler(
    IAvatarRepository avatarRepository,
    IFileStorageService fileStorageService)
    : IQueryHandler<GetAvatarQuery, Result<GetAvatarResult>>
{
    public async ValueTask<Result<GetAvatarResult>> Handle(
        GetAvatarQuery query,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AvatarSize>(query.Size, ignoreCase: true, out var avatarSize))
        {
            return DomainError.Validation(
                "INVALID_AVATAR_SIZE",
                $"Invalid size '{query.Size}'. Valid sizes: tiny, small, medium, large.");
        }

        AvatarId avatarId;
        try
        {
            avatarId = AvatarId.From(query.AvatarId);
        }
        catch (Vogen.ValueObjectValidationException)
        {
            return DomainError.Validation("INVALID_AVATAR_ID", "Invalid avatar ID.");
        }

        var avatar = await avatarRepository.GetByIdAsync(avatarId, cancellationToken);
        if (avatar is null)
        {
            return DomainError.NotFound("AVATAR_NOT_FOUND", "Avatar not found.");
        }

        var variant = avatar.GetVariant(avatarSize);
        if (variant is null)
        {
            return DomainError.NotFound("AVATAR_VARIANT_NOT_FOUND", "Avatar variant not found.");
        }

        var data = await fileStorageService.GetAsync(variant.StorageKey, cancellationToken);
        if (data is null)
        {
            return DomainError.NotFound("AVATAR_DATA_NOT_FOUND", "Avatar data not found.");
        }

        var etag = $"\"{variant.StorageKey}\"";
        return new GetAvatarResult(new MemoryStream(data), variant.MimeType, etag);
    }
}
