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
    /// Validates that the user exists in the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User ID of the authenticated user.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated or user ID cannot be found.</exception>
    Task<UserId> GetUserIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current authenticated user's email from JWT claims.
    /// </summary>
    /// <returns>Email if authenticated, null otherwise.</returns>
    Email? GetUserEmail();

    /// <summary>
    /// Gets the current authenticated user's family ID from JWT claims.
    /// Used for Row-Level Security (RLS) filtering.
    /// </summary>
    /// <returns>Family ID if present in claims, null otherwise.</returns>
    FamilyId? GetFamilyId();

    /// <summary>
    /// Tries to get the user ID without throwing exceptions.
    /// </summary>
    /// <returns>The user ID if found and valid, null otherwise.</returns>
    UserId? TryGetUserId();

    /// <summary>
    /// Tries to get the family ID without throwing exceptions.
    /// </summary>
    /// <returns>The family ID if found and valid, null otherwise.</returns>
    FamilyId? TryGetFamilyId();

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
