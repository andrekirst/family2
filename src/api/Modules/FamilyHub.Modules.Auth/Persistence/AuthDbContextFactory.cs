using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FamilyHub.Modules.Auth.Persistence;

/// <summary>
/// Design-time factory for creating AuthDbContext instances.
/// Required for EF Core tooling (migrations, database commands) to work correctly.
/// </summary>
public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        // Use a dummy connection string for design-time operations
        // The actual connection string comes from configuration at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=familyhub;Username=postgres;Password=postgres",
            npgsqlOptions =>
            {
                // Specify migrations assembly explicitly
                npgsqlOptions.MigrationsAssembly(typeof(AuthDbContext).Assembly.GetName().Name);
            })
            .UseSnakeCaseNamingConvention();

        return new AuthDbContext(optionsBuilder.Options);
    }
}
