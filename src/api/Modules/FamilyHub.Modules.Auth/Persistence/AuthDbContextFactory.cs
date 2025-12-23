using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FamilyHub.Modules.Auth.Persistence;

/// <summary>
/// Factory for creating AuthDbContext at design-time (used by EF Core migrations).
/// </summary>
public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        // Use a connection string for design-time operations
        // This will be replaced with the actual connection string from appsettings.json at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=familyhub;Username=familyhub;Password=Dev123!")
            .UseSnakeCaseNamingConvention();

        return new AuthDbContext(optionsBuilder.Options);
    }
}
