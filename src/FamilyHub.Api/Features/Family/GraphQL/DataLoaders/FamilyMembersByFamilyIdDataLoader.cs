using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Family.GraphQL.DataLoaders;

/// <summary>
/// Group DataLoader for loading FamilyMember entities grouped by FamilyId.
/// Eliminates N+1 queries when resolving members for multiple families in a single GraphQL request.
/// </summary>
public sealed class FamilyMembersByFamilyIdDataLoader : GroupedDataLoader<FamilyId, FamilyMember>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FamilyMembersByFamilyIdDataLoader(
        IServiceScopeFactory scopeFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task<ILookup<FamilyId, FamilyMember>> LoadGroupedBatchAsync(
        IReadOnlyList<FamilyId> keys, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var members = await context.FamilyMembers
            .Include(fm => fm.User)
            .Where(fm => keys.Contains(fm.FamilyId) && fm.IsActive)
            .ToListAsync(cancellationToken);

        return members.ToLookup(m => m.FamilyId);
    }
}
