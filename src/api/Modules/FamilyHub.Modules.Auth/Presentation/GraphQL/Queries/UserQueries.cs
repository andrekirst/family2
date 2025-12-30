using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Validators;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Adapters;
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
    /// <param name="userRepository">Repository to retrieve full user entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current user information.</returns>
    [Authorize] // Requires authentication
    public async Task<UserType> Me(
        [Service] ICurrentUserService currentUserService,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();

        // Validate authentication using centralized validator
        var authenticatedUserId = AuthenticationValidator.RequireAuthentication(userId, "access user information");

        // Retrieve full User entity from repository
        var user = await userRepository.GetByIdAsync(authenticatedUserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {authenticatedUserId.Value} not found.");
        }

        // Map to GraphQL type using adapter (now with full entity data)
        return UserAuthenticationAdapter.ToGraphQLType(user);
    }
}
