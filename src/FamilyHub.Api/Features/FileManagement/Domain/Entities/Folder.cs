using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Aggregate root for a folder in the file hierarchy.
/// Uses materialized path pattern for efficient subtree queries.
/// </summary>
public sealed class Folder : AggregateRoot<FolderId>
{
#pragma warning disable CS8618
    private Folder() { }
#pragma warning restore CS8618

    public static Folder Create(
        FileName name,
        FolderId? parentFolderId,
        string materializedPath,
        FamilyId familyId,
        UserId createdBy)
    {
        var folder = new Folder
        {
            Id = FolderId.New(),
            Name = name,
            ParentFolderId = parentFolderId,
            MaterializedPath = materializedPath,
            FamilyId = familyId,
            CreatedBy = createdBy,
            IsInbox = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        folder.RaiseDomainEvent(new FolderCreatedEvent(
            folder.Id, name, parentFolderId, familyId, createdBy, folder.CreatedAt));

        return folder;
    }

    /// <summary>
    /// Creates the root folder for a family. Root has no parent.
    /// </summary>
    public static Folder CreateRoot(FamilyId familyId, UserId createdBy)
    {
        var folder = new Folder
        {
            Id = FolderId.New(),
            Name = FileName.From("Root"),
            ParentFolderId = null,
            MaterializedPath = "/",
            FamilyId = familyId,
            CreatedBy = createdBy,
            IsInbox = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return folder;
    }

    /// <summary>
    /// Creates the dedicated inbox folder for a family. Cannot be deleted or renamed.
    /// </summary>
    public static Folder CreateInbox(FolderId rootFolderId, FamilyId familyId, UserId createdBy)
    {
        var folder = new Folder
        {
            Id = FolderId.New(),
            Name = FileName.From("Inbox"),
            ParentFolderId = rootFolderId,
            MaterializedPath = $"/{rootFolderId.Value}/",
            FamilyId = familyId,
            CreatedBy = createdBy,
            IsInbox = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return folder;
    }

    public FileName Name { get; private set; }
    public FolderId? ParentFolderId { get; private set; }
    public string MaterializedPath { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public UserId CreatedBy { get; private set; }
    public bool IsInbox { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Rename(FileName newName)
    {
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMaterializedPath(string newPath)
    {
        MaterializedPath = newPath;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveTo(FolderId newParentFolderId, string newMaterializedPath, UserId movedBy)
    {
        var oldParentId = ParentFolderId;
        ParentFolderId = newParentFolderId;
        MaterializedPath = newMaterializedPath;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new FolderMovedEvent(
            Id, oldParentId, newParentFolderId, FamilyId, movedBy, UpdatedAt));
    }

    public void MarkDeleted(UserId deletedBy)
    {
        RaiseDomainEvent(new FolderDeletedEvent(
            Id, Name, FamilyId, deletedBy, DateTime.UtcNow));
    }
}
