using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Development;
using FamilyHub.TestCommon.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FamilyHub.IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory that replaces PostgreSQL with InMemoryDatabase
/// and Keycloak with mock JWT authentication. Enables self-contained integration tests
/// without external infrastructure.
/// </summary>
public class FamilyHubWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace PostgreSQL with InMemoryDatabase
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase($"FamilyHubTestDb_{Guid.NewGuid()}");
            });

            // Override JWT Bearer to use mock RSA keys (no Keycloak needed)
            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                MockJwtBearerOptionsSetup.Configure);

            // Remove DevDataSeeder (requires real PostgreSQL)
            services.RemoveAll<IHostedService>();
        });
    }

    /// <summary>
    /// Creates an HttpClient with a valid mock JWT Bearer token in the Authorization header.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(
        string sub = "test-user-001",
        string email = "test@example.com",
        string name = "Test User")
    {
        var client = CreateClient();
        var token = MockJwtTokenGenerator.GenerateToken(sub, email, name);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
