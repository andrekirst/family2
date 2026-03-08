using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IFilePermissionRepository : IWriteRepository<FilePermission, FilePermissionId>
{
    Task<List<FilePermission>> GetByResourceAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken cancellationToken = default);

    Task<FilePermission?> GetByMemberAndResourceAsync(
        UserId memberId, PermissionResourceType resourceType, Guid resourceId, CancellationToken cancellationToken = default);

    Task<List<FilePermission>> GetByMemberAndFamilyAsync(
        UserId memberId, FamilyId familyId, CancellationToken cancellationToken = default);

    Task<bool> HasAnyPermissionsAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken cancellationToken = default);

    Task<List<FilePermission>> GetByFolderIdsAsync(
        IEnumerable<Guid> folderIds, CancellationToken cancellationToken = default);

    Task RemoveAsync(FilePermission permission, CancellationToken cancellationToken = default);

    Task RemoveByMemberAndFamilyAsync(UserId memberId, FamilyId familyId, CancellationToken cancellationToken = default);
}
