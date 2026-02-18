using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class ShareLinkAccessLogRepository(AppDbContext context) : IShareLinkAccessLogRepository
{
    public async Task<List<ShareLinkAccessLog>> GetByShareLinkIdAsync(ShareLinkId shareLinkId, CancellationToken ct = default)
        => await context.Set<ShareLinkAccessLog>()
            .Where(l => l.ShareLinkId == shareLinkId)
            .OrderByDescending(l => l.AccessedAt)
            .ToListAsync(ct);

    public async Task AddAsync(ShareLinkAccessLog log, CancellationToken ct = default)
        => await context.Set<ShareLinkAccessLog>().AddAsync(log, ct);
}
