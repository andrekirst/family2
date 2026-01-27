using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Repositories;

/// <summary>
/// Repository interface for authentication audit logs.
/// </summary>
public interface IAuthAuditLogRepository
{
    /// <summary>
    /// Adds a new audit log entry.
    /// </summary>
    /// <param name="auditLog">The audit log entry to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(AuthAuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="limit">Maximum number of entries to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of audit log entries.</returns>
    Task<IReadOnlyList<AuthAuditLog>> GetByUserIdAsync(UserId userId, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent failed login attempts for an email.
    /// Used for rate limiting and security monitoring.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="since">Time window to search.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of failed attempts.</returns>
    Task<int> GetFailedLoginAttemptsAsync(Email email, DateTime since, CancellationToken cancellationToken = default);
}
