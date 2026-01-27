using FamilyHub.Modules.Auth.Domain;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence;

/// <summary>
/// Database context for the Auth module.
///
/// PHASE 5 STATE: Family entities have been extracted to FamilyDbContext.
/// - Auth entities (User, OutboxEvent): Owned by Auth module
/// - User.FamilyId: References family.families.id (cross-schema, no FK constraint)
///
/// CROSS-MODULE QUERIES:
/// - Family module uses IUserLookupService for cross-module queries
/// - This maintains bounded context separation per DDD best practices
/// </summary>
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the users in the Auth module.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Gets the refresh tokens for JWT authentication.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Gets the external login providers linked to users.
    /// </summary>
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    /// <summary>
    /// Gets the authentication audit logs.
    /// </summary>
    public DbSet<AuthAuditLog> AuthAuditLogs => Set<AuthAuditLog>();

    /// <summary>
    /// Gets the outbox events for reliable domain event publishing.
    /// </summary>
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set PostgreSQL schema for this module
        modelBuilder.HasDefaultSchema("auth");

        // Apply all configurations from this assembly (auto-discovery)
        // Auth: UserConfiguration, RefreshTokenConfiguration, ExternalLoginConfiguration, AuthAuditLogConfiguration, OutboxEventConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
