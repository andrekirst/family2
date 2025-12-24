using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for user operations.
/// </summary>
[ExtendObjectType("Query")]
public sealed class UserQueries
{
    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <param name="currentUserService">Service to access current user info.</param>
    /// <returns>Current user information.</returns>
    [Authorize] // Requires authentication
    public UserType Me([Service] ICurrentUserService currentUserService)
    {
        var userId = currentUserService.GetUserId();
        var email = currentUserService.GetUserEmail();

        if (userId == null || email == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        return new UserType
        {
            Id = userId.Value.Value,
            Email = email.Value.Value,
            EmailVerified = false, // TODO: Get from user entity
            CreatedAt = DateTime.UtcNow // TODO: Get from user entity
        };
    }
}
