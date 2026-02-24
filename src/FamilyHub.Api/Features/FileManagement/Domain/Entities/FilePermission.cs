using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Represents an explicit permission grant on a file or folder for a specific user.
/// When at least one permission exists on a resource, it becomes "restricted" —
/// only users with explicit grants (or owner/admin bypass) can access it.
/// </summary>
public sealed class FilePermission : AggregateRoot<FilePermissionId>
{
#pragma warning disable CS8618
    private FilePermission() { }
#pragma warning restore CS8618

    public static FilePermission Create(
        PermissionResourceType resourceType,
        Guid resourceId,
        UserId memberId,
        FilePermissionLevel permissionLevel,
        FamilyId familyId,
        UserId grantedBy)
    {
        var permission = new FilePermission
        {
            Id = FilePermissionId.New(),
            ResourceType = resourceType,
            ResourceId = resourceId,
            MemberId = memberId,
            PermissionLevel = permissionLevel,
            FamilyId = familyId,
            GrantedBy = grantedBy,
            GrantedAt = DateTime.UtcNow
        };

        if (resourceType == PermissionResourceType.File)
        {
            permission.RaiseDomainEvent(new FilePermissionChangedEvent(
                FileId.From(resourceId), memberId, permissionLevel, grantedBy));
        }
        else
        {
            permission.RaiseDomainEvent(new FolderPermissionChangedEvent(
                FolderId.From(resourceId), memberId, permissionLevel, grantedBy));
        }

        return permission;
    }

    public PermissionResourceType ResourceType { get; private set; }
    public Guid ResourceId { get; private set; }
    public UserId MemberId { get; private set; }
    public FilePermissionLevel PermissionLevel { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public UserId GrantedBy { get; private set; }
    public DateTime GrantedAt { get; private set; }

    public void UpdateLevel(FilePermissionLevel newLevel, UserId changedBy)
    {
        PermissionLevel = newLevel;
        GrantedBy = changedBy;
        GrantedAt = DateTime.UtcNow;

        if (ResourceType == PermissionResourceType.File)
        {
            RaiseDomainEvent(new FilePermissionChangedEvent(
                FileId.From(ResourceId), MemberId, newLevel, changedBy));
        }
        else
        {
            RaiseDomainEvent(new FolderPermissionChangedEvent(
                FolderId.From(ResourceId), MemberId, newLevel, changedBy));
        }
    }
}
