using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FamilyHub.Api.Common.Database;

/// <summary>
/// Design-time factory for AppDbContext.
/// Used by EF Core tools (migrations) to create DbContext without running full application startup.
/// Avoids Roslyn version conflicts with Vogen/Wolverine during design-time operations.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Use connection string for local PostgreSQL
        var connectionString = "Host=localhost;Port=5432;Database=familyhub;Username=familyhub;Password=familyhub";

        optionsBuilder.UseNpgsql(connectionString);

        // Return AppDbContext WITHOUT IMessageBus (design-time only)
        // This avoids loading Wolverine and its dependencies during migrations
        return new AppDbContext(optionsBuilder.Options);
    }
}
