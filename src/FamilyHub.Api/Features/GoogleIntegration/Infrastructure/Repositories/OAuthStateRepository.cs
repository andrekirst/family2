using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Repositories;

public sealed class OAuthStateRepository(AppDbContext context) : IOAuthStateRepository
{
    public async Task<OAuthState?> GetByStateAsync(string state, CancellationToken cancellationToken = default)
        => await context.OAuthStates.FirstOrDefaultAsync(s => s.State == state, cancellationToken);

    public async Task AddAsync(OAuthState oauthState, CancellationToken cancellationToken = default)
        => await context.OAuthStates.AddAsync(oauthState, cancellationToken);

    public Task DeleteAsync(OAuthState oauthState, CancellationToken cancellationToken = default)
    {
        context.OAuthStates.Remove(oauthState);
        return Task.CompletedTask;
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
        => await context.OAuthStates
            .Where(s => s.ExpiresAt <= DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);
}
