using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.DataLoaders;

/// <summary>
/// Groups and batches invitation lookups by FamilyId into a single database query.
/// Used for resolving Family.Invitations (1:N relationship) efficiently.
/// Uses IDbContextFactory for proper DbContext pooling to avoid conflicts
/// when DataLoader executes after the main request scope is disposed.
/// </summary>
/// <param name="dbContextFactory">Factory for creating Family DbContext instances.</param>
/// <param name="batchScheduler">Scheduler for batching DataLoader requests.</param>
/// <param name="options">Options for DataLoader configuration.</param>
public sealed class InvitationsByFamilyGroupedDataLoader(
    IDbContextFactory<FamilyDbContext> dbContextFactory,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options) : GroupedDataLoader<FamilyId, FamilyMemberInvitation>(batchScheduler, options)
{
    /// <inheritdoc />
    protected override async Task<ILookup<FamilyId, FamilyMemberInvitation>> LoadGroupedBatchAsync(
        IReadOnlyList<FamilyId> keys,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Single query with WHERE family_id IN (...) for all requested families
        // The Vogen EfCoreValueConverter handles FamilyId <-> Guid conversion
        var invitations = await dbContext.FamilyMemberInvitations
            .Where(i => keys.Contains(i.FamilyId))
            .ToListAsync(cancellationToken);

        // Group invitations by their FamilyId for 1:N relationship resolution
        return invitations.ToLookup(i => i.FamilyId);
    }
}
