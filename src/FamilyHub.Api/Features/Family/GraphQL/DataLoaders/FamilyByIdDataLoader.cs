using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using GreenDonut;
using Microsoft.EntityFrameworkCore;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Api.Features.Family.GraphQL.DataLoaders;

/// <summary>
/// Batch DataLoader for loading Family entities by their IDs.
/// Eliminates N+1 queries when multiple Family references are resolved in a single GraphQL request.
/// </summary>
public sealed class FamilyByIdDataLoader : BatchDataLoader<FamilyId, FamilyEntity>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FamilyByIdDataLoader(
        IServiceScopeFactory scopeFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task<IReadOnlyDictionary<FamilyId, FamilyEntity>> LoadBatchAsync(
        IReadOnlyList<FamilyId> keys, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var families = await context.Families
            .Where(f => keys.Contains(f.Id))
            .ToListAsync(cancellationToken);

        return families.ToDictionary(f => f.Id);
    }
}
