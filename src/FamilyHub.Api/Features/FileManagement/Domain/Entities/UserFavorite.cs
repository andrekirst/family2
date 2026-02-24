using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

public sealed class UserFavorite
{
    private UserFavorite() { }

    public static UserFavorite Create(UserId userId, FileId fileId)
    {
        return new UserFavorite
        {
            UserId = userId,
            FileId = fileId,
            FavoritedAt = DateTime.UtcNow
        };
    }

    public UserId UserId { get; private set; }
    public FileId FileId { get; private set; }
    public DateTime FavoritedAt { get; private set; }
}
