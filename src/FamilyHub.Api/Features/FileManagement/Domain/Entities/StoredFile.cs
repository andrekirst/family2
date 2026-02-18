using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Aggregate root for a stored file's metadata.
/// Binary data is managed by IStorageProvider via the StorageKey reference.
/// </summary>
public sealed class StoredFile : AggregateRoot<FileId>
{
#pragma warning disable CS8618
    private StoredFile() { }
#pragma warning restore CS8618

    public static StoredFile Create(
        FileName name,
        MimeType mimeType,
        FileSize size,
        StorageKey storageKey,
        Checksum checksum,
        FolderId folderId,
        FamilyId familyId,
        UserId uploadedBy)
    {
        var file = new StoredFile
        {
            Id = FileId.New(),
            Name = name,
            MimeType = mimeType,
            Size = size,
            StorageKey = storageKey,
            Checksum = checksum,
            FolderId = folderId,
            FamilyId = familyId,
            UploadedBy = uploadedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        file.RaiseDomainEvent(new FileUploadedEvent(
            file.Id, familyId, storageKey.Value, mimeType, size, checksum));

        return file;
    }

    public FileName Name { get; private set; }
    public MimeType MimeType { get; private set; }
    public FileSize Size { get; private set; }
    public StorageKey StorageKey { get; private set; }
    public Checksum Checksum { get; private set; }
    public FolderId FolderId { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public UserId UploadedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Rename(FileName newName, UserId renamedBy)
    {
        var oldName = Name;
        Name = newName;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new FileRenamedEvent(
            Id, oldName, newName, FamilyId, renamedBy, UpdatedAt));
    }

    public void MoveTo(FolderId newFolderId, UserId movedBy)
    {
        var fromFolderId = FolderId;
        FolderId = newFolderId;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new FileMovedEvent(
            Id, fromFolderId, newFolderId, FamilyId, movedBy, UpdatedAt));
    }

    public void MarkDeleted(UserId deletedBy)
    {
        RaiseDomainEvent(new FileDeletedEvent(Id, FamilyId, StorageKey.Value));
    }
}
