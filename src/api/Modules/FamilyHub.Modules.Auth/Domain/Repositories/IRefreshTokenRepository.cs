using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Repositories;

/// <summary>
/// Repository interface for refresh token persistence.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Adds a new refresh token to the database.
    /// </summary>
    /// <param name="refreshToken">The refresh token to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a refresh token by its hash.
    /// </summary>
    /// <param name="tokenHash">The SHA256 hash of the token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The refresh token if found, null otherwise.</returns>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active (non-revoked, non-expired) refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active refresh tokens.</returns>
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes expired refresh tokens (cleanup job).
    /// </summary>
    /// <param name="olderThan">Delete tokens that expired before this date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of tokens deleted.</returns>
    Task<int> DeleteExpiredTokensAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}
