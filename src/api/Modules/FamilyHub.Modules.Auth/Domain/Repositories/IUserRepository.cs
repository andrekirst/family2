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

    /// <summary>
    /// Updates an existing user asynchronously with SaveChanges.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their password reset token.
    /// </summary>
    /// <param name="token">The password reset token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found with a valid token; otherwise, null.</returns>
    Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email verification token.
    /// </summary>
    /// <param name="token">The email verification token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found with a valid token; otherwise, null.</returns>
    Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default);
}
