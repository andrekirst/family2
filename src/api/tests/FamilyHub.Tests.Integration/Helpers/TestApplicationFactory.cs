using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        // Ensure database is migrated (runs synchronously during construction)
        EnsureDatabaseMigrated();
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

    private void EnsureDatabaseMigrated()
    {
        using var scope = Services.CreateScope();
        var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        authDbContext.Database.Migrate();
    }
}
