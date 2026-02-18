using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class ShareLinkRepository(AppDbContext context) : IShareLinkRepository
{
    public async Task<ShareLink?> GetByIdAsync(ShareLinkId id, CancellationToken ct = default)
        => await context.Set<ShareLink>().FindAsync([id], cancellationToken: ct);

    public async Task<ShareLink?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await context.Set<ShareLink>()
            .FirstOrDefaultAsync(s => s.Token == token, ct);

    public async Task<List<ShareLink>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => await context.Set<ShareLink>()
            .Where(s => s.FamilyId == familyId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<ShareLink>> GetActiveByResourceIdAsync(Guid resourceId, CancellationToken ct = default)
        => await context.Set<ShareLink>()
            .Where(s => s.ResourceId == resourceId && !s.IsRevoked)
            .ToListAsync(ct);

    public async Task AddAsync(ShareLink link, CancellationToken ct = default)
        => await context.Set<ShareLink>().AddAsync(link, ct);
}
