using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

public sealed class FileTag
{
    private FileTag() { }

    public static FileTag Create(FileId fileId, TagId tagId, DateTimeOffset utcNow)
    {
        return new FileTag
        {
            FileId = fileId,
            TagId = tagId,
            CreatedAt = utcNow.UtcDateTime
        };
    }

    public FileId FileId { get; private set; }
    public TagId TagId { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
