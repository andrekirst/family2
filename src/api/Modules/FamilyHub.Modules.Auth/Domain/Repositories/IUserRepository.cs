using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate root.
/// Follows DDD repository pattern - operates on aggregate roots only.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their external OAuth provider ID.
    /// </summary>
    /// <param name="externalProvider">The OAuth provider name (e.g., "zitadel").</param>
    /// <param name="externalUserId">The user ID from the external provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByExternalProviderAsync(
        string externalProvider,
        string externalUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their external user ID and provider.
    /// </summary>
    /// <param name="externalUserId">The user ID from the external provider.</param>
    /// <param name="externalProvider">The OAuth provider name (e.g., "zitadel").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByExternalUserIdAsync(
        string externalUserId,
        string externalProvider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with the given email already exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a user with this email exists; otherwise, false.</returns>
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user in the repository.
    /// Note: In EF Core with change tracking, explicit Update call may not be necessary.
    /// </summary>
    /// <param name="user">The user to update.</param>
    void Update(User user);

    /// <summary>
    /// Removes a user from the repository (hard delete).
    /// Note: Prefer using User.Delete() for soft deletes.
    /// </summary>
    /// <param name="user">The user to remove.</param>
    void Remove(User user);
}
