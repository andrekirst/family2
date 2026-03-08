using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class FilePermissionRepository(AppDbContext context) : IFilePermissionRepository
{
    public async Task<FilePermission?> GetByIdAsync(FilePermissionId id, CancellationToken cancellationToken = default)
        => await context.Set<FilePermission>().FindAsync([id], cancellationToken: cancellationToken);

    public async Task<bool> ExistsByIdAsync(FilePermissionId id, CancellationToken cancellationToken = default)
        => await context.Set<FilePermission>().AnyAsync(p => p.Id == id, cancellationToken);

    public async Task<List<FilePermission>> GetByResourceAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken cancellationToken = default)
        => await context.Set<FilePermission>()
            .Where(p => p.ResourceType == resourceType && p.ResourceId == resourceId)
            .ToListAsync(cancellationToken);

    public async Task<FilePermission?> GetByMemberAndResourceAsync(
        UserId memberId, PermissionResourceType resourceType, Guid resourceId, CancellationToken cancellationToken = default)
        => await context.Set<FilePermission>()
            .FirstOrDefaultAsync(p =>
                p.MemberId == memberId &&
                p.ResourceType == resourceType &&
                p.ResourceId == resourceId, cancellationToken);

    public async Task<List<FilePermission>> GetByMemberAndFamilyAsync(
        UserId memberId, FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<FilePermission>()
            .Where(p => p.MemberId == memberId && p.FamilyId == familyId)
            .ToListAsync(cancellationToken);

    public async Task<bool> HasAnyPermissionsAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken cancellationToken = default)
        => await context.Set<FilePermission>()
            .AnyAsync(p => p.ResourceType == resourceType && p.ResourceId == resourceId, cancellationToken);

    public async Task<List<FilePermission>> GetByFolderIdsAsync(
        IEnumerable<Guid> folderIds, CancellationToken cancellationToken = default)
    {
        var ids = folderIds.ToList();
        return await context.Set<FilePermission>()
            .Where(p => p.ResourceType == PermissionResourceType.Folder && ids.Contains(p.ResourceId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FilePermission permission, CancellationToken cancellationToken = default)
        => await context.Set<FilePermission>().AddAsync(permission, cancellationToken);

    public Task RemoveAsync(FilePermission permission, CancellationToken cancellationToken = default)
    {
        context.Set<FilePermission>().Remove(permission);
        return Task.CompletedTask;
    }

    public async Task RemoveByMemberAndFamilyAsync(UserId memberId, FamilyId familyId, CancellationToken cancellationToken = default)
    {
        var permissions = await context.Set<FilePermission>()
            .Where(p => p.MemberId == memberId && p.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        context.Set<FilePermission>().RemoveRange(permissions);
    }
}
