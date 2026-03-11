using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.BaseData.Domain.Entities;
using FamilyHub.Api.Features.BaseData.Domain.Repositories;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.BaseData.Infrastructure.Repositories;

public sealed class FederalStateRepository(AppDbContext context) : IFederalStateRepository
{
    public async Task<List<FederalState>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.FederalStates
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<FederalState?> GetByIso3166CodeAsync(Iso3166Code code, CancellationToken cancellationToken = default)
    {
        return await context.FederalStates
            .FirstOrDefaultAsync(f => f.Iso3166Code == code, cancellationToken);
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await context.FederalStates.AnyAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<FederalState> entities, CancellationToken cancellationToken = default)
    {
        await context.FederalStates.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
