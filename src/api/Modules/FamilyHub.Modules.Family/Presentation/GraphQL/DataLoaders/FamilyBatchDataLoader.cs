using FamilyHub.Modules.Family.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using GreenDonut;
using Microsoft.EntityFrameworkCore;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.DataLoaders;

/// <summary>
/// Batches multiple family lookups by ID into a single database query.
/// Uses IDbContextFactory for proper DbContext pooling to avoid conflicts
/// when DataLoader executes after the main request scope is disposed.
/// </summary>
public sealed class FamilyBatchDataLoader : BatchDataLoader<FamilyId, FamilyAggregate>
{
    private readonly IDbContextFactory<FamilyDbContext> _dbContextFactory;

    public FamilyBatchDataLoader(
        IDbContextFactory<FamilyDbContext> dbContextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options)
        : base(batchScheduler, options)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task<IReadOnlyDictionary<FamilyId, FamilyAggregate>> LoadBatchAsync(
        IReadOnlyList<FamilyId> keys,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Single query with WHERE id IN (...) for all requested families
        // The Vogen EfCoreValueConverter handles FamilyId <-> Guid conversion
        return await dbContext.Families
            .Where(f => keys.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, cancellationToken);
    }
}
