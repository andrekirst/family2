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
/// <param name="dbContextFactory">Factory for creating Family DbContext instances.</param>
/// <param name="batchScheduler">Scheduler for batching DataLoader requests.</param>
/// <param name="options">Options for DataLoader configuration.</param>
public sealed class FamilyBatchDataLoader(
    IDbContextFactory<FamilyDbContext> dbContextFactory,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options) : BatchDataLoader<FamilyId, FamilyAggregate>(batchScheduler, options)
{
    /// <inheritdoc />
    protected override async Task<IReadOnlyDictionary<FamilyId, FamilyAggregate>> LoadBatchAsync(
        IReadOnlyList<FamilyId> keys,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Single query with WHERE id IN (...) for all requested families
        // The Vogen EfCoreValueConverter handles FamilyId <-> Guid conversion
        return await dbContext.Families
            .Where(f => keys.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, cancellationToken);
    }
}
