using EFCore.NamingConventions.Internal;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Custom WebApplicationFactory for integration tests that provides TestCurrentUserService.
/// Zitadel mock configuration is set up by TestEnvironmentSetup module initializer.
/// </summary>
public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainerFixture _containerFixture;

    public TestApplicationFactory(PostgreSqlContainerFixture containerFixture)
    {
        _containerFixture = containerFixture;
    }

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
            services.AddPooledDbContextFactory<AuthDbContext>((serviceProvider, options) =>
                options.UseNpgsql(_containerFixture.ConnectionString, npgsqlOptions =>
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
    private class MigrationHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public MigrationHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            // Check all migrations
            var allMigrations = dbContext.Database.GetMigrations();
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);

            Console.WriteLine($"[Migration Debug] Total migrations: {allMigrations.Count()}");
            Console.WriteLine($"[Migration Debug] All migrations: {string.Join(", ", allMigrations)}");
            Console.WriteLine($"[Migration Debug] Pending migrations: {string.Join(", ", pendingMigrations)}");
            Console.WriteLine($"[Migration Debug] Applied migrations: {string.Join(", ", appliedMigrations)}");

            // Apply migrations
            await dbContext.Database.MigrateAsync(cancellationToken);

            // Verify schema exists
            var hasSchema = await dbContext.Database.CanConnectAsync(cancellationToken);
            Console.WriteLine($"[Migration Debug] Database connection: {hasSchema}");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
