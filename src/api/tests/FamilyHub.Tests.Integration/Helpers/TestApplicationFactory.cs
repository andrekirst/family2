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
        // Use unique database name in CI to avoid conflicts between parallel test execution
        var dbName = Environment.GetEnvironmentVariable("CI") == "true"
            ? $"familyhub_test_{Guid.NewGuid():N}"  // Unique DB per test class in CI
            : "familyhub_test";

        var connectionString = $"Host=localhost;Port=5432;Database={dbName};Username=postgres;Password=Dev123!;Include Error Detail=true";
        Environment.SetEnvironmentVariable("ConnectionStrings__FamilyHubDb", connectionString);

        // Force server/host creation to trigger migrations in CreateHost()
        // This ensures database is ready before tests access Services property
        _ = Server;
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

        // Get connection string for diagnostics
        var connectionString = authDbContext.Database.GetConnectionString();

        try
        {
            // Apply migrations (handles database creation automatically)
            var logPath = "/tmp/test-factory-migrations.log";
            File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] TEST-FACTORY: Applying migrations to: {connectionString}\n");

            authDbContext.Database.Migrate();

            File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] TEST-FACTORY: Migrations applied successfully\n");

            // Verify schema exists
            var canConnect = authDbContext.Database.CanConnect();
            File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] TEST-FACTORY: Can connect: {canConnect}\n");
        }
        catch (Exception ex)
        {
            var logPath = "/tmp/test-factory-migrations.log";
            File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] TEST-FACTORY: MIGRATION FAILED: {ex.Message}\n");
            File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] TEST-FACTORY: Stack trace: {ex.StackTrace}\n");
            throw;
        }

        return host;
    }
}
