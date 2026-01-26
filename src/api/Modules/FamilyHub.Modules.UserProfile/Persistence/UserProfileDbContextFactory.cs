using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FamilyHub.Modules.UserProfile.Persistence;

/// <summary>
/// Design-time factory for creating UserProfileDbContext instances.
/// Required for EF Core tooling (migrations, database commands) to work correctly.
/// </summary>
public class UserProfileDbContextFactory : IDesignTimeDbContextFactory<UserProfileDbContext>
{
    /// <inheritdoc />
    public UserProfileDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserProfileDbContext>();

        // Use a dummy connection string for design-time operations
        // The actual connection string comes from configuration at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=familyhub;Username=postgres;Password=postgres",
            npgsqlOptions =>
            {
                // Specify migrations assembly explicitly
                npgsqlOptions.MigrationsAssembly(typeof(UserProfileDbContext).Assembly.GetName().Name);
            })
            .UseSnakeCaseNamingConvention();

        return new UserProfileDbContext(optionsBuilder.Options);
    }
}
