using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Represents a generated thumbnail for a file (image or video keyframe).
/// Multiple thumbnail sizes may exist per file (e.g., 200x200 small, 800x800 large).
/// </summary>
public sealed class FileThumbnail : AggregateRoot<FileThumbnailId>
{
#pragma warning disable CS8618
    private FileThumbnail() { }
#pragma warning restore CS8618

    public static FileThumbnail Create(
        FileId fileId,
        int width,
        int height,
        StorageKey storageKey)
    {
        var thumbnail = new FileThumbnail
        {
            Id = FileThumbnailId.New(),
            FileId = fileId,
            Width = width,
            Height = height,
            StorageKey = storageKey,
            GeneratedAt = DateTime.UtcNow
        };

        thumbnail.RaiseDomainEvent(new ThumbnailGeneratedEvent(
            fileId, storageKey, width, height));

        return thumbnail;
    }

    public FileId FileId { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public StorageKey StorageKey { get; private set; }
    public DateTime GeneratedAt { get; private set; }
}
