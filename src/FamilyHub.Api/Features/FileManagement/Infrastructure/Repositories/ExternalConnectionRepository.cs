using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class ExternalConnectionRepository(AppDbContext context) : IExternalConnectionRepository
{
    public async Task<ExternalConnection?> GetByIdAsync(ExternalConnectionId id, CancellationToken cancellationToken = default)
        => await context.Set<ExternalConnection>().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> ExistsByIdAsync(ExternalConnectionId id, CancellationToken cancellationToken = default)
        => await context.Set<ExternalConnection>().AnyAsync(c => c.Id == id, cancellationToken);

    public async Task<List<ExternalConnection>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<ExternalConnection>()
            .Where(c => c.FamilyId == familyId)
            .OrderBy(c => c.ProviderType)
            .ToListAsync(cancellationToken);

    public async Task<ExternalConnection?> GetByFamilyAndProviderAsync(
        FamilyId familyId, ExternalProviderType providerType, CancellationToken cancellationToken = default)
        => await context.Set<ExternalConnection>()
            .FirstOrDefaultAsync(c => c.FamilyId == familyId && c.ProviderType == providerType, cancellationToken);

    public async Task AddAsync(ExternalConnection connection, CancellationToken cancellationToken = default)
        => await context.Set<ExternalConnection>().AddAsync(connection, cancellationToken);

    public Task RemoveAsync(ExternalConnection connection, CancellationToken cancellationToken = default)
    {
        context.Set<ExternalConnection>().Remove(connection);
        return Task.CompletedTask;
    }
}
