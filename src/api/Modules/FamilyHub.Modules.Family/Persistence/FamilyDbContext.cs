using FamilyHub.Modules.Family.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Family.Persistence;

/// <summary>
/// Database context for the Family module.
///
/// PHASE 5 STATE: This DbContext owns Family module entities:
/// - Family aggregate: Core family entity
/// - FamilyMemberInvitation aggregate: Invitation management
///
/// SCHEMA: All tables reside in the "family" PostgreSQL schema.
///
/// CROSS-SCHEMA REFERENCES:
/// - Family.OwnerId references auth.users.id (no FK constraint, ID-only)
/// - FamilyMemberInvitation.InvitedByUserId references auth.users.id (no FK constraint, ID-only)
/// - Cross-module queries handled via IUserLookupService abstraction
///
/// ARCHITECTURE NOTES:
/// - One DbContext per module enforces bounded context boundaries
/// - Uses pooled DbContext factory for performance optimization
/// - Applies snake_case naming convention for PostgreSQL compatibility
/// </summary>
public class FamilyDbContext(DbContextOptions<FamilyDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Family aggregates.
    /// </summary>
    public DbSet<Domain.Aggregates.Family> Families => Set<Domain.Aggregates.Family>();

    /// <summary>
    /// Family member invitations.
    /// </summary>
    public DbSet<FamilyMemberInvitation> FamilyMemberInvitations => Set<FamilyMemberInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set PostgreSQL schema for this module
        modelBuilder.HasDefaultSchema("family");

        // Apply all configurations from this assembly (auto-discovery)
        // Discovers: FamilyConfiguration, FamilyMemberInvitationConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FamilyDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
