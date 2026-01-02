using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Abstractions;

/// <summary>
/// Service for accessing the current authenticated user's information from JWT claims.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current authenticated user's ID.
    /// </summary>
    /// <returns>User ID of the authenticated user.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated or user ID cannot be found.</exception>
    UserId GetUserId();

    /// <summary>
    /// Gets the current authenticated user's ID asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User ID of the authenticated user.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated or user ID cannot be found.</exception>
    Task<UserId> GetUserIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current authenticated user's email.
    /// </summary>
    /// <returns>Email if authenticated, null otherwise.</returns>
    Email? GetUserEmail();

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
