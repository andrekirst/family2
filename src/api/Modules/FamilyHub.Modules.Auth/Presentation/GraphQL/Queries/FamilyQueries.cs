using FamilyDomain = FamilyHub.Modules.Family.Domain;
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
    /// Gets the current authenticated user's family.
    /// Returns null if the user is not found.
    /// Uses HotChocolate projections for automatic field selection optimization.
    /// </summary>
    /// <param name="dbContext">Auth module database context.</param>
    /// <param name="currentUserService">Service to access current user info.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's family, or null if the user is not found.</returns>
    [Authorize]
    [GraphQLDescription("Get the current user's family")]
    [UseProjection]
    public async Task<FamilyDomain.Family?> Family(
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

        // Return the user's family using the one-to-many relationship
        return await dbContext.Families

            .FirstOrDefaultAsync(f => f.Id == user.FamilyId, cancellationToken);
    }
}
