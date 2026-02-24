using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Services;

/// <summary>
/// Permission resolution for files and folders.
/// Default = unrestricted (all family members can access).
/// When explicit permissions exist on a resource, it becomes restricted.
/// Resolution chain: admin bypass → owner bypass → direct → folder inheritance (first match wins).
/// </summary>
public sealed class FileManagementAuthorizationService(
    IFilePermissionRepository permissionRepository,
    IStoredFileRepository storedFileRepository,
    IFolderRepository folderRepository,
    IFamilyMemberRepository familyMemberRepository) : IFileManagementAuthorizationService
{
    public async Task<bool> HasFilePermissionAsync(
        UserId userId, FileId fileId, FilePermissionLevel requiredLevel,
        FamilyId familyId, CancellationToken ct = default)
    {
        // 1. Family Owner/Admin bypass
        if (await IsFamilyAdminOrOwnerAsync(userId, familyId, ct))
            return true;

        // 2. File owner always has Manage
        var file = await storedFileRepository.GetByIdAsync(fileId, ct);
        if (file is not null && file.UploadedBy == userId)
            return true;

        // 3. Check if the file has any direct permissions (is it restricted?)
        var fileRestricted = await permissionRepository.HasAnyPermissionsAsync(
            PermissionResourceType.File, fileId.Value, ct);

        if (fileRestricted)
        {
            // Check direct file permission
            var directPerm = await permissionRepository.GetByMemberAndResourceAsync(
                userId, PermissionResourceType.File, fileId.Value, ct);

            if (directPerm is not null)
                return directPerm.PermissionLevel >= requiredLevel;

            // Direct restriction exists but user has no grant — denied at file level
            // Still check folder inheritance below
        }

        // 4. Walk up folder hierarchy (file's folder → parent → ... → root)
        if (file is not null)
        {
            var hasInherited = await CheckFolderInheritanceAsync(
                userId, file.FolderId, requiredLevel, ct);

            if (hasInherited.HasValue)
                return hasInherited.Value;
        }

        // 5. No restrictions found anywhere → default unrestricted
        return !fileRestricted;
    }

    public async Task<bool> HasFolderPermissionAsync(
        UserId userId, FolderId folderId, FilePermissionLevel requiredLevel,
        FamilyId familyId, CancellationToken ct = default)
    {
        // 1. Family Owner/Admin bypass
        if (await IsFamilyAdminOrOwnerAsync(userId, familyId, ct))
            return true;

        // 2. Folder creator always has Manage
        var folder = await folderRepository.GetByIdAsync(folderId, ct);
        if (folder is not null && folder.CreatedBy == userId)
            return true;

        // 3. Check direct folder permission
        var folderRestricted = await permissionRepository.HasAnyPermissionsAsync(
            PermissionResourceType.Folder, folderId.Value, ct);

        if (folderRestricted)
        {
            var directPerm = await permissionRepository.GetByMemberAndResourceAsync(
                userId, PermissionResourceType.Folder, folderId.Value, ct);

            if (directPerm is not null)
                return directPerm.PermissionLevel >= requiredLevel;
        }

        // 4. Walk up parent folders
        if (folder?.ParentFolderId is not null)
        {
            var hasInherited = await CheckFolderInheritanceAsync(
                userId, folder.ParentFolderId.Value, requiredLevel, ct);

            if (hasInherited.HasValue)
                return hasInherited.Value;
        }

        // 5. No restrictions found → default unrestricted
        return !folderRestricted;
    }

    public async Task<bool> IsResourceRestrictedAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken ct = default)
    {
        return await permissionRepository.HasAnyPermissionsAsync(resourceType, resourceId, ct);
    }

    /// <summary>
    /// Walks up the folder hierarchy checking for permissions.
    /// Returns true if the user has sufficient permission, false if restricted without grant,
    /// or null if no restrictions were found (unrestricted).
    /// </summary>
    private async Task<bool?> CheckFolderInheritanceAsync(
        UserId userId, FolderId startFolderId, FilePermissionLevel requiredLevel, CancellationToken ct)
    {
        // Get ancestors from the start folder up to root
        var ancestors = await folderRepository.GetAncestorsAsync(startFolderId, ct);

        // Include the start folder itself in the chain
        var startFolder = await folderRepository.GetByIdAsync(startFolderId, ct);
        var folderChain = new List<FolderId>();

        if (startFolder is not null)
            folderChain.Add(startFolder.Id);

        folderChain.AddRange(ancestors.Select(a => a.Id));

        // Walk from nearest to root (first match wins)
        foreach (var folderId in folderChain)
        {
            var hasPermissions = await permissionRepository.HasAnyPermissionsAsync(
                PermissionResourceType.Folder, folderId.Value, ct);

            if (!hasPermissions)
                continue;

            // This folder is restricted — check user's grant
            var perm = await permissionRepository.GetByMemberAndResourceAsync(
                userId, PermissionResourceType.Folder, folderId.Value, ct);

            return perm is not null && perm.PermissionLevel >= requiredLevel;
        }

        // No restricted folders in the chain
        return null;
    }

    private async Task<bool> IsFamilyAdminOrOwnerAsync(
        UserId userId, FamilyId familyId, CancellationToken ct)
    {
        var member = await familyMemberRepository.GetByUserAndFamilyAsync(userId, familyId, ct);
        return member is not null && member.IsActive &&
            (member.Role.CanEditFamily()); // Owner and Admin both return true for CanEditFamily
    }
}
