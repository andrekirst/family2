using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.DataLoaders;

/// <summary>
/// Groups and batches user lookups by FamilyId into a single database query.
/// Used for resolving Family.Members (1:N relationship) efficiently.
/// Uses IDbContextFactory for proper DbContext pooling to avoid conflicts
/// when DataLoader executes after the main request scope is disposed.
/// </summary>
/// <param name="dbContextFactory">Factory for creating Auth DbContext instances.</param>
/// <param name="batchScheduler">Scheduler for batching DataLoader requests.</param>
/// <param name="options">Options for DataLoader configuration.</param>
public sealed class UsersByFamilyGroupedDataLoader(
    IDbContextFactory<AuthDbContext> dbContextFactory,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options) : GroupedDataLoader<FamilyId, User>(batchScheduler, options)
{
    /// <inheritdoc />
    protected override async Task<ILookup<FamilyId, User>> LoadGroupedBatchAsync(
        IReadOnlyList<FamilyId> keys,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Single query with WHERE family_id IN (...) for all requested families
        // The Vogen EfCoreValueConverter handles FamilyId <-> Guid conversion
        var users = await dbContext.Users
            .Where(u => keys.Contains(u.FamilyId))
            .ToListAsync(cancellationToken);

        // Group users by their FamilyId for 1:N relationship resolution
        return users.ToLookup(u => u.FamilyId);
    }
}
