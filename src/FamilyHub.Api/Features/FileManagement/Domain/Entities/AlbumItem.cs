using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

public sealed class AlbumItem
{
    private AlbumItem() { }

    public static AlbumItem Create(AlbumId albumId, FileId fileId, UserId addedBy)
    {
        return new AlbumItem
        {
            AlbumId = albumId,
            FileId = fileId,
            AddedBy = addedBy,
            AddedAt = DateTime.UtcNow
        };
    }

    public AlbumId AlbumId { get; private set; }
    public FileId FileId { get; private set; }
    public UserId AddedBy { get; private set; }
    public DateTime AddedAt { get; private set; }
}
