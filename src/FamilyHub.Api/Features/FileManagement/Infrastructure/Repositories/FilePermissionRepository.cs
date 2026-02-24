using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class FilePermissionRepository(AppDbContext context) : IFilePermissionRepository
{
    public async Task<FilePermission?> GetByIdAsync(FilePermissionId id, CancellationToken ct = default)
        => await context.Set<FilePermission>().FindAsync([id], cancellationToken: ct);

    public async Task<List<FilePermission>> GetByResourceAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken ct = default)
        => await context.Set<FilePermission>()
            .Where(p => p.ResourceType == resourceType && p.ResourceId == resourceId)
            .ToListAsync(ct);

    public async Task<FilePermission?> GetByMemberAndResourceAsync(
        UserId memberId, PermissionResourceType resourceType, Guid resourceId, CancellationToken ct = default)
        => await context.Set<FilePermission>()
            .FirstOrDefaultAsync(p =>
                p.MemberId == memberId &&
                p.ResourceType == resourceType &&
                p.ResourceId == resourceId, ct);

    public async Task<List<FilePermission>> GetByMemberAndFamilyAsync(
        UserId memberId, FamilyId familyId, CancellationToken ct = default)
        => await context.Set<FilePermission>()
            .Where(p => p.MemberId == memberId && p.FamilyId == familyId)
            .ToListAsync(ct);

    public async Task<bool> HasAnyPermissionsAsync(
        PermissionResourceType resourceType, Guid resourceId, CancellationToken ct = default)
        => await context.Set<FilePermission>()
            .AnyAsync(p => p.ResourceType == resourceType && p.ResourceId == resourceId, ct);

    public async Task<List<FilePermission>> GetByFolderIdsAsync(
        IEnumerable<Guid> folderIds, CancellationToken ct = default)
    {
        var ids = folderIds.ToList();
        return await context.Set<FilePermission>()
            .Where(p => p.ResourceType == PermissionResourceType.Folder && ids.Contains(p.ResourceId))
            .ToListAsync(ct);
    }

    public async Task AddAsync(FilePermission permission, CancellationToken ct = default)
        => await context.Set<FilePermission>().AddAsync(permission, ct);

    public Task RemoveAsync(FilePermission permission, CancellationToken ct = default)
    {
        context.Set<FilePermission>().Remove(permission);
        return Task.CompletedTask;
    }

    public async Task RemoveByMemberAndFamilyAsync(UserId memberId, FamilyId familyId, CancellationToken ct = default)
    {
        var permissions = await context.Set<FilePermission>()
            .Where(p => p.MemberId == memberId && p.FamilyId == familyId)
            .ToListAsync(ct);

        context.Set<FilePermission>().RemoveRange(permissions);
    }
}
