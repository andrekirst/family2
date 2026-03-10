using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Auth.GraphQL.DataLoaders;

/// <summary>
/// Batch DataLoader for loading User entities by their IDs.
/// Eliminates N+1 queries when multiple User references are resolved in a single GraphQL request.
/// </summary>
public sealed class UserByIdDataLoader : BatchDataLoader<UserId, User>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UserByIdDataLoader(
        IServiceScopeFactory scopeFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task<IReadOnlyDictionary<UserId, User>> LoadBatchAsync(
        IReadOnlyList<UserId> keys, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var users = await context.Users
            .Where(u => keys.Contains(u.Id))
            .ToListAsync(cancellationToken);

        return users.ToDictionary(u => u.Id);
    }
}
