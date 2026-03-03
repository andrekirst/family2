using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Domain.Entities;

/// <summary>
/// Owned entity representing a file attached to a message.
/// References a FileId from the (future) File Management module.
/// Uses plain Guid for Id since owned entities are always accessed through their parent aggregate.
/// </summary>
public sealed class MessageAttachment
{
#pragma warning disable CS8618
    private MessageAttachment() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public FileId FileId { get; private set; }
    public string FileName { get; private set; }
    public string MimeType { get; private set; }
    public long FileSize { get; private set; }
    public DateTime AttachedAt { get; private set; }

    public static MessageAttachment Create(FileId fileId, string fileName, string mimeType, long fileSize)
    {
        return new MessageAttachment
        {
            Id = Guid.NewGuid(),
            FileId = fileId,
            FileName = fileName,
            MimeType = mimeType,
            FileSize = fileSize,
            AttachedAt = DateTime.UtcNow
        };
    }
}
