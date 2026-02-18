using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Represents a single version of a file. Each re-upload or restore creates a new version.
/// The version with IsCurrent=true is the active version displayed to users.
/// Versions are immutable snapshots — restoring creates a new version rather than rewinding.
/// </summary>
public sealed class FileVersion : AggregateRoot<FileVersionId>
{
#pragma warning disable CS8618
    private FileVersion() { }
#pragma warning restore CS8618

    public static FileVersion Create(
        FileId fileId,
        int versionNumber,
        StorageKey storageKey,
        FileSize fileSize,
        Checksum checksum,
        UserId uploadedBy,
        bool isCurrent = true)
    {
        var version = new FileVersion
        {
            Id = FileVersionId.New(),
            FileId = fileId,
            VersionNumber = versionNumber,
            StorageKey = storageKey,
            FileSize = fileSize,
            Checksum = checksum,
            UploadedBy = uploadedBy,
            IsCurrent = isCurrent,
            UploadedAt = DateTime.UtcNow
        };

        version.RaiseDomainEvent(new FileVersionCreatedEvent(
            fileId, version.Id, versionNumber, uploadedBy, fileSize));

        return version;
    }

    /// <summary>
    /// Creates a new version by restoring from a previous version's data.
    /// Raises both FileVersionCreatedEvent and FileVersionRestoredEvent.
    /// </summary>
    public static FileVersion CreateFromRestore(
        FileId fileId,
        int versionNumber,
        FileVersionId restoredFromVersionId,
        StorageKey storageKey,
        FileSize fileSize,
        Checksum checksum,
        UserId restoredBy)
    {
        var version = new FileVersion
        {
            Id = FileVersionId.New(),
            FileId = fileId,
            VersionNumber = versionNumber,
            StorageKey = storageKey,
            FileSize = fileSize,
            Checksum = checksum,
            UploadedBy = restoredBy,
            IsCurrent = true,
            UploadedAt = DateTime.UtcNow
        };

        version.RaiseDomainEvent(new FileVersionCreatedEvent(
            fileId, version.Id, versionNumber, restoredBy, fileSize));

        version.RaiseDomainEvent(new FileVersionRestoredEvent(
            fileId, restoredFromVersionId, version.Id, versionNumber, restoredBy));

        return version;
    }

    public FileId FileId { get; private set; }
    public int VersionNumber { get; private set; }
    public StorageKey StorageKey { get; private set; }
    public FileSize FileSize { get; private set; }
    public Checksum Checksum { get; private set; }
    public UserId UploadedBy { get; private set; }
    public bool IsCurrent { get; private set; }
    public DateTime UploadedAt { get; private set; }

    public void MarkAsNotCurrent()
    {
        IsCurrent = false;
    }

    public void MarkAsCurrent()
    {
        IsCurrent = true;
    }
}
