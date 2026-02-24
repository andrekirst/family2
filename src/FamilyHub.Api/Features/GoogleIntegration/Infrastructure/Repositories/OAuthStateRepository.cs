using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Repositories;

public sealed class OAuthStateRepository(AppDbContext context) : IOAuthStateRepository
{
    public async Task<OAuthState?> GetByStateAsync(string state, CancellationToken ct = default)
        => await context.OAuthStates.FirstOrDefaultAsync(s => s.State == state, ct);

    public async Task AddAsync(OAuthState oauthState, CancellationToken ct = default)
        => await context.OAuthStates.AddAsync(oauthState, ct);

    public Task DeleteAsync(OAuthState oauthState, CancellationToken ct = default)
    {
        context.OAuthStates.Remove(oauthState);
        return Task.CompletedTask;
    }

    public async Task DeleteExpiredAsync(CancellationToken ct = default)
        => await context.OAuthStates
            .Where(s => s.ExpiresAt <= DateTime.UtcNow)
            .ExecuteDeleteAsync(ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);
}
