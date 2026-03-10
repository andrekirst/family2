using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Repositories;

public sealed class GoogleAccountLinkRepository(AppDbContext context) : IGoogleAccountLinkRepository
{
    public async Task<GoogleAccountLink?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
        => await context.GoogleAccountLinks.FirstOrDefaultAsync(l => l.UserId == userId, cancellationToken);

    public async Task<GoogleAccountLink?> GetByGoogleAccountIdAsync(
        GoogleAccountId googleAccountId, CancellationToken cancellationToken = default)
        => await context.GoogleAccountLinks.FirstOrDefaultAsync(
            l => l.GoogleAccountId == googleAccountId, cancellationToken);

    public async Task<List<GoogleAccountLink>> GetExpiringTokensAsync(
        DateTime expiringBefore, CancellationToken cancellationToken = default)
        => await context.GoogleAccountLinks
            .Where(l => l.Status == GoogleLinkStatus.Active && l.AccessTokenExpiresAt <= expiringBefore)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(GoogleAccountLink link, CancellationToken cancellationToken = default)
        => await context.GoogleAccountLinks.AddAsync(link, cancellationToken);

    public Task UpdateAsync(GoogleAccountLink link, CancellationToken cancellationToken = default)
    {
        // EF Core change tracker detects modifications automatically
        return Task.CompletedTask;
    }

    public Task DeleteAsync(GoogleAccountLink link, CancellationToken cancellationToken = default)
    {
        context.GoogleAccountLinks.Remove(link);
        return Task.CompletedTask;
    }
}
