using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Repositories;

public sealed class GoogleAccountLinkRepository(AppDbContext context) : IGoogleAccountLinkRepository
{
    public async Task<GoogleAccountLink?> GetByUserIdAsync(UserId userId, CancellationToken ct = default)
        => await context.GoogleAccountLinks.FirstOrDefaultAsync(l => l.UserId == userId, ct);

    public async Task<GoogleAccountLink?> GetByGoogleAccountIdAsync(
        GoogleAccountId googleAccountId, CancellationToken ct = default)
        => await context.GoogleAccountLinks.FirstOrDefaultAsync(
            l => l.GoogleAccountId == googleAccountId, ct);

    public async Task<List<GoogleAccountLink>> GetExpiringTokensAsync(
        DateTime expiringBefore, CancellationToken ct = default)
        => await context.GoogleAccountLinks
            .Where(l => l.Status == GoogleLinkStatus.Active && l.AccessTokenExpiresAt <= expiringBefore)
            .ToListAsync(ct);

    public async Task AddAsync(GoogleAccountLink link, CancellationToken ct = default)
        => await context.GoogleAccountLinks.AddAsync(link, ct);

    public Task UpdateAsync(GoogleAccountLink link, CancellationToken ct = default)
    {
        context.GoogleAccountLinks.Update(link);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(GoogleAccountLink link, CancellationToken ct = default)
    {
        context.GoogleAccountLinks.Remove(link);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);
}
