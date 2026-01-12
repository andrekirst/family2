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
public sealed class UserBatchDataLoader : BatchDataLoader<UserId, User>
{
    private readonly IDbContextFactory<AuthDbContext> _dbContextFactory;

    public UserBatchDataLoader(
        IDbContextFactory<AuthDbContext> dbContextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options)
        : base(batchScheduler, options)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task<IReadOnlyDictionary<UserId, User>> LoadBatchAsync(
        IReadOnlyList<UserId> keys,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Single query with WHERE id IN (...) for all requested users
        // The Vogen EfCoreValueConverter handles UserId <-> Guid conversion
        return await dbContext.Users
            .Where(u => keys.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);
    }
}
