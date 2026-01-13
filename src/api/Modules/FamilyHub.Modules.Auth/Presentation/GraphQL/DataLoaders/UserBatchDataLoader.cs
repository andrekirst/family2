using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.DataLoaders;

/// <summary>
/// Batches multiple user lookups by ID into a single database query.
/// Uses IDbContextFactory for proper DbContext pooling to avoid conflicts
/// when DataLoader executes after the main request scope is disposed.
/// </summary>
/// <param name="dbContextFactory">Factory for creating Auth DbContext instances.</param>
/// <param name="batchScheduler">Scheduler for batching DataLoader requests.</param>
/// <param name="options">Options for DataLoader configuration.</param>
public sealed class UserBatchDataLoader(
    IDbContextFactory<AuthDbContext> dbContextFactory,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options) : BatchDataLoader<UserId, User>(batchScheduler, options)
{
    /// <inheritdoc />
    protected override async Task<IReadOnlyDictionary<UserId, User>> LoadBatchAsync(
        IReadOnlyList<UserId> keys,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Single query with WHERE id IN (...) for all requested users
        // The Vogen EfCoreValueConverter handles UserId <-> Guid conversion
        return await dbContext.Users
            .Where(u => keys.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);
    }
}
