using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.DataLoaders;

/// <summary>
/// Groups and batches user lookups by FamilyId into a single database query.
/// Used for resolving Family.Members (1:N relationship) efficiently.
/// Uses IDbContextFactory for proper DbContext pooling to avoid conflicts
/// when DataLoader executes after the main request scope is disposed.
/// </summary>
public sealed class UsersByFamilyGroupedDataLoader : GroupedDataLoader<FamilyId, User>
{
    private readonly IDbContextFactory<AuthDbContext> _dbContextFactory;

    public UsersByFamilyGroupedDataLoader(
        IDbContextFactory<AuthDbContext> dbContextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options)
        : base(batchScheduler, options)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task<ILookup<FamilyId, User>> LoadGroupedBatchAsync(
        IReadOnlyList<FamilyId> keys,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Single query with WHERE family_id IN (...) for all requested families
        // The Vogen EfCoreValueConverter handles FamilyId <-> Guid conversion
        var users = await dbContext.Users
            .Where(u => keys.Contains(u.FamilyId))
            .ToListAsync(cancellationToken);

        // Group users by their FamilyId for 1:N relationship resolution
        return users.ToLookup(u => u.FamilyId);
    }
}
