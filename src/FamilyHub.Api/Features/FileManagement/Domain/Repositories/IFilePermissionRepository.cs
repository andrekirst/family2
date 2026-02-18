using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFilePermissionRepository
{
    Task<FilePermission?> GetByIdAsync(FilePermissionId id, CancellationToken ct = default);

    Task<List<FilePermission>> GetByResourceAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken ct = default);

    Task<FilePermission?> GetByMemberAndResourceAsync(
        UserId memberId, PermissionResourceType resourceType, Guid resourceId, CancellationToken ct = default);

    Task<List<FilePermission>> GetByMemberAndFamilyAsync(
        UserId memberId, FamilyId familyId, CancellationToken ct = default);

    Task<bool> HasAnyPermissionsAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken ct = default);

    Task<List<FilePermission>> GetByFolderIdsAsync(
        IEnumerable<Guid> folderIds, CancellationToken ct = default);

    Task AddAsync(FilePermission permission, CancellationToken ct = default);

    Task RemoveAsync(FilePermission permission, CancellationToken ct = default);

    Task RemoveByMemberAndFamilyAsync(UserId memberId, FamilyId familyId, CancellationToken ct = default);
}
