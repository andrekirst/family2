using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence.Repositories;

/// <summary>
/// EF Core repository implementation for authentication audit logs.
/// </summary>
public sealed class AuthAuditLogRepository : IAuthAuditLogRepository
{
    private readonly AuthDbContext _context;

    /// <summary>
    /// Initializes a new instance of the AuthAuditLogRepository.
    /// </summary>
    /// <param name="context">The Auth database context.</param>
    public AuthAuditLogRepository(AuthDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(AuthAuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuthAuditLogs.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AuthAuditLog>> GetByUserIdAsync(UserId userId, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.AuthAuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.OccurredAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetFailedLoginAttemptsAsync(Email email, DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.AuthAuditLogs
            .Where(a => a.Email == email &&
                        a.EventType == AuthAuditEventType.FAILED_LOGIN &&
                        a.OccurredAt >= since)
            .CountAsync(cancellationToken);
    }
}
