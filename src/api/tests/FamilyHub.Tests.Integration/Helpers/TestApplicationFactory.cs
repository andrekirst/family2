using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Family.Persistence;
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

            // Remove existing FamilyDbContext registrations
            services.RemoveAll<DbContextOptions<FamilyDbContext>>();
            services.RemoveAll<FamilyDbContext>();
            services.RemoveAll<IDbContextFactory<FamilyDbContext>>();

            // Add FamilyDbContext with test container connection string
            services.AddPooledDbContextFactory<FamilyDbContext>((_, options) =>
                options.UseNpgsql(containerFixture.ConnectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(FamilyDbContext).Assembly.GetName().Name);
                    })
                    .UseSnakeCaseNamingConvention());

            // Also register scoped FamilyDbContext
            services.AddScoped(sp =>
            {
                var factory = sp.GetRequiredService<IDbContextFactory<FamilyDbContext>>();
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
    /// Hosted service that applies EF Core schemas on startup.
    /// Creates both Auth and Family schemas for cross-module testing.
    /// Handles cases where schemas already exist (e.g., created by PostgreSqlContainerFixture).
    /// </summary>
    private class MigrationHostedService(IServiceProvider serviceProvider) : IHostedService
    {
        // Static flag to ensure schemas are only created once per test run
        private static bool _schemasCreated;
        private static readonly object Lock = new();

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Quick check without lock for performance
            if (_schemasCreated)
                return;

            lock (Lock)
            {
                // Double-check after acquiring lock
                if (_schemasCreated)
                    return;

                _schemasCreated = true;
            }

            using var scope = serviceProvider.CreateScope();

            // Create Auth schema
            var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await authDbContext.Database.EnsureCreatedAsync(cancellationToken);

            // Create Family schema using SQL script
            // Catch "already exists" error in case PostgreSqlContainerFixture already created it
            var familyDbContext = scope.ServiceProvider.GetRequiredService<FamilyDbContext>();
            try
            {
                var script = familyDbContext.Database.GenerateCreateScript();
                await familyDbContext.Database.ExecuteSqlRawAsync(script, cancellationToken);
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07") // 42P07 = duplicate_table
            {
                // Schema already exists - this is expected when PostgreSqlContainerFixture runs first
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
