using EFCore.NamingConventions.Internal;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Custom WebApplicationFactory for integration tests that provides TestCurrentUserService.
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
            // Remove the existing DbContext registrations (including factory and pool)
            services.RemoveAll<DbContextOptions<AuthDbContext>>();
            services.RemoveAll<AuthDbContext>();
            services.RemoveAll<IDbContextFactory<AuthDbContext>>();
            services.RemoveAll(typeof(Microsoft.EntityFrameworkCore.Internal.IDbContextPool<>).MakeGenericType(typeof(AuthDbContext)));

            // Add AuthDbContext with test container connection string
            // Use AddPooledDbContextFactory to match production setup (singleton-safe)
            services.AddPooledDbContextFactory<AuthDbContext>((serviceProvider, options) =>
                options.UseNpgsql(_containerFixture.ConnectionString)
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

            // Build service provider and apply migrations
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            dbContext.Database.Migrate();
        });
    }
}
