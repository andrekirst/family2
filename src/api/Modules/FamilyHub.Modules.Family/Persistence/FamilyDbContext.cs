using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Family.Persistence;

/// <summary>
/// Database context for the Family module.
/// IMPORTANT: Uses "auth" schema for Phase 0 safety strategy.
/// Database schema will be migrated to "family" in Phase 1 after code is battle-tested.
/// </summary>
public class FamilyDbContext(DbContextOptions<FamilyDbContext> options) : DbContext(options)
{
    public DbSet<global::FamilyHub.Modules.Family.Domain.Family> Families => Set<global::FamilyHub.Modules.Family.Domain.Family>();
    public DbSet<global::FamilyHub.Modules.Family.Domain.FamilyMemberInvitation> FamilyMemberInvitations => Set<global::FamilyHub.Modules.Family.Domain.FamilyMemberInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // CRITICAL: Use "auth" schema for Phase 0 (zero-risk migration)
        // This points to the existing tables in the auth schema
        // Migration to "family" schema will happen in Phase 1
        modelBuilder.HasDefaultSchema("auth");

        // Apply all configurations from this assembly
        // Vogen value converters are explicitly configured in Fluent API configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FamilyDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
