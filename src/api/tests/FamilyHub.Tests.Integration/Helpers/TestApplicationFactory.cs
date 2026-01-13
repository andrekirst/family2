using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Custom WebApplicationFactory for integration tests that provides TestCurrentUserService.
/// Zitadel mock configuration is set up by TestEnvironmentSetup module initializer.
/// </summary>
public sealed class TestApplicationFactory(PostgreSqlContainerFixture containerFixture) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registrations
            services.RemoveAll<DbContextOptions<AuthDbContext>>();
            services.RemoveAll<AuthDbContext>();
            services.RemoveAll<IDbContextFactory<AuthDbContext>>();
            // Note: IDbContextPool is internal - EF Core will handle pool cleanup automatically

            // Add AuthDbContext with test container connection string
            // Use AddPooledDbContextFactory to match production setup (singleton-safe)
            services.AddPooledDbContextFactory<AuthDbContext>((_, options) =>
                options.UseNpgsql(containerFixture.ConnectionString, npgsqlOptions =>
                    {
                        // Specify migrations assembly explicitly
                        // Migrations are in FamilyHub.Modules.Auth assembly, not test assembly
                        npgsqlOptions.MigrationsAssembly(typeof(AuthDbContext).Assembly.GetName().Name);
                    })
                    .UseSnakeCaseNamingConvention());

            // Also register scoped DbContext for UnitOfWork pattern (same as production)
            services.AddScoped(sp =>
            {
                var factory = sp.GetRequiredService<IDbContextFactory<AuthDbContext>>();
                return factory.CreateDbContext();
            });

            // Remove the existing ICurrentUserService registration
            services.RemoveAll<ICurrentUserService>();

            // Register TestCurrentUserService as a singleton
            services.AddSingleton<ICurrentUserService, TestCurrentUserService>();
        });

        // Apply migrations AFTER host is built (not during service configuration)
        builder.ConfigureServices(services =>
        {
            // Register a hosted service that applies migrations on startup
            services.AddHostedService<MigrationHostedService>();
        });
    }

    /// <summary>
    /// Hosted service that applies EF Core migrations on startup.
    /// This runs after all services are registered and the host is built.
    /// </summary>
    private class MigrationHostedService(IServiceProvider serviceProvider) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            // Debug: Check migrations assembly configuration
            var assemblyName = typeof(AuthDbContext).Assembly.GetName().Name;
            Console.WriteLine($"[Migration Debug] AuthDbContext Assembly: {assemblyName}");
            Console.WriteLine($"[Migration Debug] AuthDbContext Assembly Full Name: {typeof(AuthDbContext).Assembly.FullName}");

            // Check if migration types exist in the assembly
            var authAssembly = typeof(AuthDbContext).Assembly;
            var migrationTypes = authAssembly.GetTypes()
                .Where(t => t.Namespace == "FamilyHub.Modules.Auth.Migrations" && !t.Name.Contains("Snapshot"))
                .ToList();
            Console.WriteLine($"[Migration Debug] Migration types found in assembly: {migrationTypes.Count}");
            foreach (var type in migrationTypes)
            {
                Console.WriteLine($"[Migration Debug]   - {type.FullName}");
            }

            // Check all migrations
            var allMigrations = dbContext.Database.GetMigrations();
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);

            Console.WriteLine($"[Migration Debug] Total migrations: {allMigrations.Count()}");
            Console.WriteLine($"[Migration Debug] All migrations: {string.Join(", ", allMigrations)}");
            Console.WriteLine($"[Migration Debug] Pending migrations: {string.Join(", ", pendingMigrations)}");
            Console.WriteLine($"[Migration Debug] Applied migrations: {string.Join(", ", appliedMigrations)}");

            // Apply migrations
            // NOTE: Using EnsureCreated() instead of MigrateAsync() as a workaround for
            // EF Core migration discovery issues. See PostgreSqlContainerFixture for explanation.
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            // Verify schema exists
            var hasSchema = await dbContext.Database.CanConnectAsync(cancellationToken);
            Console.WriteLine($"[Migration Debug] Database connection: {hasSchema}");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
