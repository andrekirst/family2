using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.UserProfile.Persistence;

/// <summary>
/// Database context for the UserProfile module.
///
/// PHASE 1 STATE: This DbContext owns UserProfile module entities.
/// Currently a foundation setup - entities will be added in subsequent issues.
///
/// SCHEMA: All tables reside in the "user_profile" PostgreSQL schema.
///
/// CROSS-SCHEMA REFERENCES:
/// - UserProfile entities will reference auth.users.id (no FK constraint, ID-only)
/// - Cross-module queries handled via IUserLookupService abstraction
///
/// ARCHITECTURE NOTES:
/// - One DbContext per module enforces bounded context boundaries
/// - Uses snake_case naming convention for PostgreSQL compatibility
/// </summary>
public class UserProfileDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileDbContext"/> class.
    /// Used by migrations and scenarios where domain event dispatching is not needed.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public UserProfileDbContext(DbContextOptions<UserProfileDbContext> options) : base(options)
    {
    }

    // DbSets will be added here when entities are implemented
    // Example: public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set PostgreSQL schema for this module
        modelBuilder.HasDefaultSchema("user_profile");

        // Apply all configurations from this assembly (auto-discovery)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserProfileDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
