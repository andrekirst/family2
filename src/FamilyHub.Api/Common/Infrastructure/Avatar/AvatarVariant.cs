using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// A specific size variant of an avatar image.
/// Contains metadata and a storage key to retrieve the binary data.
/// </summary>
public sealed class AvatarVariant
{
    // Private parameterless constructor for EF Core
    private AvatarVariant() { }

    public Guid Id { get; private set; }
    public AvatarId AvatarId { get; private set; }
    public AvatarSize Size { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public int FileSize { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    // Navigation property
    public AvatarAggregate Avatar { get; private set; } = null!;

    internal static AvatarVariant Create(
        AvatarId avatarId,
        AvatarSize size,
        string storageKey,
        string mimeType,
        int fileSize,
        int width,
        int height)
    {
        return new AvatarVariant
        {
            Id = Guid.NewGuid(),
            AvatarId = avatarId,
            Size = size,
            StorageKey = storageKey,
            MimeType = mimeType,
            FileSize = fileSize,
            Width = width,
            Height = height
        };
    }
}
