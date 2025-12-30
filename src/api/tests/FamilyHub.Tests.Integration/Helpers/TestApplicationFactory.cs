using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Persistence;
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
/// </summary>

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    public TestApplicationFactory()
    {
        // Set environment variables BEFORE Program.cs runs

        // Zitadel OAuth configuration
        Environment.SetEnvironmentVariable("Zitadel__Authority", "https://test.zitadel.cloud");
        Environment.SetEnvironmentVariable("Zitadel__ClientId", "test-client-id");
        Environment.SetEnvironmentVariable("Zitadel__ClientSecret", "test-client-secret");
        Environment.SetEnvironmentVariable("Zitadel__RedirectUri", "https://localhost:5001/callback");
        Environment.SetEnvironmentVariable("Zitadel__Scope", "openid profile email");
        Environment.SetEnvironmentVariable("Zitadel__Audience", "test-client-id");

        // Database connection string (GitHub Actions postgres service or local)
        var connectionString = Environment.GetEnvironmentVariable("CI") == "true"
            ? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=Dev123!"
            : "Host=localhost;Port=5432;Database=familyhub_test;Username=postgres;Password=Dev123!";

        Environment.SetEnvironmentVariable("ConnectionStrings__FamilyHubDb", connectionString);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing ICurrentUserService registration
            services.RemoveAll<ICurrentUserService>();

            // Register TestCurrentUserService as a singleton
            services.AddSingleton<ICurrentUserService, TestCurrentUserService>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Run migrations after host is created
        using var scope = host.Services.CreateScope();
        var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        try
        {
            var connectionString = authDbContext.Database.GetConnectionString();
            Console.WriteLine($"[TEST-FACTORY] Connection string: {connectionString}");

            // Ensure clean schema for each test run
            Console.WriteLine("[TEST-FACTORY] Dropping auth schema...");
            authDbContext.Database.ExecuteSqlRaw("DROP SCHEMA IF EXISTS auth CASCADE");
            Console.WriteLine("[TEST-FACTORY] Auth schema dropped successfully");

            Console.WriteLine("[TEST-FACTORY] Applying EF Core migrations...");
            authDbContext.Database.Migrate();
            Console.WriteLine("[TEST-FACTORY] Migrations applied successfully");

            // Verify schema exists
            var canConnect = authDbContext.Database.CanConnect();
            Console.WriteLine($"[TEST-FACTORY] Can connect to database: {canConnect}");

            // Verify auth schema was created
            var schemaExists = authDbContext.Database.ExecuteSqlRaw(@"
                SELECT 1 FROM information_schema.schemata WHERE schema_name = 'auth'
            ");
            Console.WriteLine($"[TEST-FACTORY] Auth schema exists: {schemaExists >= 0}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TEST-FACTORY] MIGRATION FAILED: {ex.Message}");
            Console.WriteLine($"[TEST-FACTORY] Stack trace: {ex.StackTrace}");
            throw;
        }

        return host;
    }
}
