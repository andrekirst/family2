using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for family operations.
/// </summary>
[ExtendObjectType("Query")]
public sealed class FamilyQueries
{
    /// <summary>
    /// Gets the current authenticated user's active family.
    /// Returns null if the user has no families.
    /// Uses HotChocolate projections for automatic field selection optimization.
    /// </summary>
    /// <param name="dbContext">Auth module database context.</param>
    /// <param name="currentUserService">Service to access current user info.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's current family, or null if they have no families.</returns>
    [Authorize]
    [GraphQLDescription("Get the current user's active family")]
    [UseProjection]
    public async Task<Family?> Family(
        [Service] AuthDbContext dbContext,
        [Service] ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var userId = await currentUserService.GetUserIdAsync(cancellationToken);

        return await dbContext.Families
            .Include(f => f.UserFamilies)
            .Where(f => f.UserFamilies.Any(uf => uf.UserId == userId && uf.IsCurrentFamily))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
