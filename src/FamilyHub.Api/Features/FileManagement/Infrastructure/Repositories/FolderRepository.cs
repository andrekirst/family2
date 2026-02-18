using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class FolderRepository(AppDbContext context) : IFolderRepository
{
    public async Task<Folder?> GetByIdAsync(FolderId id, CancellationToken ct = default)
        => await context.Set<Folder>().FindAsync([id], cancellationToken: ct);

    public async Task<Folder?> GetRootFolderAsync(FamilyId familyId, CancellationToken ct = default)
        => await context.Set<Folder>()
            .FirstOrDefaultAsync(f => f.FamilyId == familyId && f.ParentFolderId == null, ct);

    public async Task<List<Folder>> GetChildrenAsync(FolderId parentId, CancellationToken ct = default)
        => await context.Set<Folder>()
            .Where(f => f.ParentFolderId == parentId)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Folder folder, CancellationToken ct = default)
        => await context.Set<Folder>().AddAsync(folder, ct);

    public Task RemoveAsync(Folder folder, CancellationToken ct = default)
    {
        context.Set<Folder>().Remove(folder);
        return Task.CompletedTask;
    }
}
