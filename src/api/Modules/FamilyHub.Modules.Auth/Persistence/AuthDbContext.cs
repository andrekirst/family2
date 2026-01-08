using FamilyHub.Modules.Auth.Domain;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence;

/// <summary>
/// Database context for the Auth module.
/// </summary>
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<FamilyHub.Modules.Family.Domain.Family> Families => Set<FamilyHub.Modules.Family.Domain.Family>();
    public DbSet<FamilyHub.Modules.Family.Domain.FamilyMemberInvitation> FamilyMemberInvitations => Set<FamilyHub.Modules.Family.Domain.FamilyMemberInvitation>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set PostgreSQL schema for this module
        modelBuilder.HasDefaultSchema("auth");

        // Apply all configurations from this assembly
        // Vogen value converters are explicitly configured in Fluent API configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}