using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FamilyHub.Api.Common.Database;

/// <summary>
/// Design-time factory for AppDbContext.
/// Used by EF Core tools (migrations) to create DbContext without running full application startup.
/// Avoids Roslyn version conflicts with Vogen during design-time operations.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Use connection string for local PostgreSQL
        var connectionString = "Host=localhost;Port=5432;Database=familyhub;Username=familyhub;Password=familyhub";

        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();

        // Return AppDbContext for design-time only (no DI container)
        return new AppDbContext(optionsBuilder.Options);
    }
}
