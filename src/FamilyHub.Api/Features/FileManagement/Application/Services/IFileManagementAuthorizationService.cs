using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Services;

/// <summary>
/// Authorization service for file and folder access control.
/// By default all resources are visible to all family members.
/// When explicit permissions exist on a resource, it becomes restricted
/// and only granted users (or owner/admin bypass) can access it.
/// </summary>
public interface IFileManagementAuthorizationService
{
    /// <summary>
    /// Checks whether a user has at least the specified permission level on a file.
    /// Resolution chain: admin bypass → owner bypass → direct file permission → folder inheritance.
    /// </summary>
    Task<bool> HasFilePermissionAsync(
        UserId userId, FileId fileId, FilePermissionLevel requiredLevel,
        FamilyId familyId, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a user has at least the specified permission level on a folder.
    /// Resolution chain: admin bypass → owner bypass → direct folder permission → parent inheritance.
    /// </summary>
    Task<bool> HasFolderPermissionAsync(
        UserId userId, FolderId folderId, FilePermissionLevel requiredLevel,
        FamilyId familyId, CancellationToken ct = default);

    /// <summary>
    /// Returns true if the resource has any explicit permissions (i.e. is restricted).
    /// </summary>
    Task<bool> IsResourceRestrictedAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken ct = default);
}
