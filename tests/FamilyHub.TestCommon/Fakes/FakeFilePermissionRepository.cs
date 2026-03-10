using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFilePermissionRepository : IFilePermissionRepository
{
    public List<FilePermission> Permissions { get; } = [];

    public Task<FilePermission?> GetByIdAsync(FilePermissionId id, CancellationToken cancellationToken = default)
        => Task.FromResult(Permissions.FirstOrDefault(p => p.Id == id));

    public Task<bool> ExistsByIdAsync(FilePermissionId id, CancellationToken cancellationToken = default)
        => Task.FromResult(Permissions.Any(p => p.Id == id));

    public Task<List<FilePermission>> GetByResourceAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken cancellationToken = default)
        => Task.FromResult(Permissions
            .Where(p => p.ResourceType == resourceType && p.ResourceId == resourceId)
            .ToList());

    public Task<FilePermission?> GetByMemberAndResourceAsync(
        UserId memberId, PermissionResourceType resourceType, Guid resourceId, CancellationToken cancellationToken = default)
        => Task.FromResult(Permissions.FirstOrDefault(p =>
            p.MemberId == memberId &&
            p.ResourceType == resourceType &&
            p.ResourceId == resourceId));

    public Task<List<FilePermission>> GetByMemberAndFamilyAsync(
        UserId memberId, FamilyId familyId, CancellationToken cancellationToken = default)
        => Task.FromResult(Permissions
            .Where(p => p.MemberId == memberId && p.FamilyId == familyId)
            .ToList());

    public Task<bool> HasAnyPermissionsAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken cancellationToken = default)
        => Task.FromResult(Permissions
            .Any(p => p.ResourceType == resourceType && p.ResourceId == resourceId));

    public Task<List<FilePermission>> GetByFolderIdsAsync(
        IEnumerable<Guid> folderIds, CancellationToken cancellationToken = default)
    {
        var ids = folderIds.ToList();
        return Task.FromResult(Permissions
            .Where(p => p.ResourceType == PermissionResourceType.Folder && ids.Contains(p.ResourceId))
            .ToList());
    }

    public Task AddAsync(FilePermission permission, CancellationToken cancellationToken = default)
    {
        Permissions.Add(permission);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(FilePermission permission, CancellationToken cancellationToken = default)
    {
        Permissions.Remove(permission);
        return Task.CompletedTask;
    }

    public Task RemoveByMemberAndFamilyAsync(UserId memberId, FamilyId familyId, CancellationToken cancellationToken = default)
    {
        Permissions.RemoveAll(p => p.MemberId == memberId && p.FamilyId == familyId);
        return Task.CompletedTask;
    }
}
