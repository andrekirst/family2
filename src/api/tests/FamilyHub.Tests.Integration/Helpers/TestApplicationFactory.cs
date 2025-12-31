using FamilyHub.Modules.Auth.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Custom WebApplicationFactory for integration tests with Testcontainers PostgreSQL support.
/// Accepts connection string from PostgreSqlContainerFixture to use real PostgreSQL instance.
/// </summary>
public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    /// <summary>
    /// Creates a new test application factory with the specified PostgreSQL connection string.
    /// </summary>
    /// <param name="connectionString">Connection string from Testcontainers PostgreSQL fixture</param>
    public TestApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;

        // Set Zitadel OAuth configuration environment variables
        Environment.SetEnvironmentVariable("Zitadel__Authority", "https://test.zitadel.cloud");
        Environment.SetEnvironmentVariable("Zitadel__ClientId", "test-client-id");
        Environment.SetEnvironmentVariable("Zitadel__ClientSecret", "test-client-secret");
        Environment.SetEnvironmentVariable("Zitadel__RedirectUri", "https://localhost:5001/callback");
        Environment.SetEnvironmentVariable("Zitadel__Scope", "openid profile email");
        Environment.SetEnvironmentVariable("Zitadel__Audience", "test-client-id");

        // Force server creation (triggers configuration)
        _ = Server;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override database connection string with Testcontainers instance
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:FamilyHubDb"] = _connectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace ICurrentUserService with TestCurrentUserService for authentication mocking
            services.RemoveAll<ICurrentUserService>();
            services.AddSingleton<ICurrentUserService, TestCurrentUserService>();
        });
    }
}
