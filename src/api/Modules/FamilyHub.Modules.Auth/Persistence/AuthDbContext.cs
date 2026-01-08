using FamilyHub.Modules.Auth.Domain;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence;

/// <summary>
/// Database context for the Auth module.
///
/// PHASE 3 STATE: This DbContext includes both Auth and Family entities.
/// - Auth entities (User, OutboxEvent): Owned by Auth module
/// - Family entities (Family, FamilyMemberInvitation): Owned by Family module but stored here
///
/// COUPLING NOTES:
/// - Family module entities are included here for pragmatic shared database access
/// - All entities remain in "auth" schema to avoid migration complexity
/// - Family entity configurations are in Auth module's Configurations folder (auto-discovered)
/// - Family repositories (in Auth module) implement Family module's repository interfaces
/// - This coupling will be resolved in Phase 5+ when we introduce separate DbContexts
///
/// ARCHITECTURE DECISION:
/// We keep the persistence layer physically in Auth module to avoid circular dependencies
/// while maintaining logical separation by:
/// 1. Repositories implement interfaces from Family module
/// 2. Configurations reference Family module's domain entities
/// 3. Service registration happens in both modules (Auth registers repos, Family declares interfaces)
/// </summary>
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    // Auth module entities
    public DbSet<User> Users => Set<User>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    // Family module entities (PHASE 3 COUPLING: temporarily stored in Auth DbContext)
    public DbSet<FamilyAggregate> Families => Set<FamilyAggregate>();
    public DbSet<FamilyMemberInvitationAggregate> FamilyMemberInvitations => Set<FamilyMemberInvitationAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set PostgreSQL schema for this module
        modelBuilder.HasDefaultSchema("auth");

        // Apply all configurations from this assembly (auto-discovery)
        // PHASE 3: This now includes both Auth and Family entity configurations:
        // - Auth: UserConfiguration, OutboxEventConfiguration
        // - Family: FamilyConfiguration, FamilyMemberInvitationConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}