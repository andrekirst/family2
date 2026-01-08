using FamilyHub.Modules.Auth.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FamilyHub.Modules.Auth.Persistence;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for family operations.
/// PHASE 4: Remains in Auth module temporarily because it requires ICurrentUserService (Auth context).
/// Uses Family domain aggregate from Family module.
/// TODO Phase 5+: Move to Family module when proper context abstraction is implemented.
/// </summary>
[ExtendObjectType("Query")]
public sealed class FamilyQueries
{
    /// <summary>
    /// Gets the current authenticated user's family.
    /// Returns null if the user is not found.
    /// Uses HotChocolate projections for automatic field selection optimization.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Get the current user's family")]
    [UseProjection]
    public async Task<FamilyAggregate?> Family(
        [Service] AuthDbContext dbContext,
        [Service] ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var userId = await currentUserService.GetUserIdAsync(cancellationToken);

        // Get the user first to retrieve their FamilyId
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        // Return the user's family (FamilyAggregate from Family module via global using)
        return await dbContext.Families
            .FirstOrDefaultAsync(f => f.Id == user.FamilyId, cancellationToken);
    }
}
