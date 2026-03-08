using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class ShareLinkRepository(AppDbContext context) : IShareLinkRepository
{
    public async Task<ShareLink?> GetByIdAsync(ShareLinkId id, CancellationToken cancellationToken = default)
        => await context.Set<ShareLink>().FindAsync([id], cancellationToken: cancellationToken);

    public async Task<bool> ExistsByIdAsync(ShareLinkId id, CancellationToken cancellationToken = default)
        => await context.Set<ShareLink>().AnyAsync(s => s.Id == id, cancellationToken);

    public async Task<ShareLink?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => await context.Set<ShareLink>()
            .FirstOrDefaultAsync(s => s.Token == token, cancellationToken);

    public async Task<List<ShareLink>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<ShareLink>()
            .Where(s => s.FamilyId == familyId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<ShareLink>> GetActiveByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken = default)
        => await context.Set<ShareLink>()
            .Where(s => s.ResourceId == resourceId && !s.IsRevoked)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ShareLink link, CancellationToken cancellationToken = default)
        => await context.Set<ShareLink>().AddAsync(link, cancellationToken);
}
