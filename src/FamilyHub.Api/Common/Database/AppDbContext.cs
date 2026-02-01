using FamilyHub.Api.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;
using FamilyEntity = FamilyHub.Api.Features.Family.Models.Family;

namespace FamilyHub.Api.Common.Database;

/// <summary>
/// Application database context for Family Hub
/// Uses PostgreSQL with schema separation for organization
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Users authenticated via OAuth (Keycloak)
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Family households
    /// </summary>
    public DbSet<FamilyEntity> Families { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure PostgreSQL schemas exist
        modelBuilder.HasDefaultSchema("public");

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Override SaveChanges to automatically update UpdatedAt timestamps
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update UpdatedAt timestamps
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is User user)
            {
                user.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is FamilyEntity family)
            {
                family.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
