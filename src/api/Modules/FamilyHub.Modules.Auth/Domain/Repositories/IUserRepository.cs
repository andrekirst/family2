using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;

namespace FamilyHub.Modules.Auth.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate root.
/// Follows DDD repository pattern - operates on aggregate roots only.
/// Extends ISpecificationRepository to support specification-based queries.
/// </summary>
public interface IUserRepository : ISpecificationRepository<User, UserId>
{
    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    [Obsolete("Use FindOneAsync with UserByEmailSpecification.")]
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their external OAuth provider ID.
    /// </summary>
    /// <param name="externalProvider">The OAuth provider name (e.g., "zitadel").</param>
    /// <param name="externalUserId">The user ID from the external provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    [Obsolete("Use FindOneAsync with UserByExternalProviderSpecification.")]
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
    [Obsolete("Use FindOneAsync with UserByExternalProviderSpecification.")]
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
    [Obsolete("Use AnyAsync with UserByEmailSpecification.")]
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users belonging to a specific family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of users in the family.</returns>
    [Obsolete("Use FindAllAsync with UsersByFamilySpecification.")]
    Task<List<User>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);
}
