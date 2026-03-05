using FamilyHub.Api.Common.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilyHub.IntegrationTests.Fixtures;

/// <summary>
/// WebApplicationFactory that uses real Keycloak and PostgreSQL from Testcontainers.
/// Provides full-fidelity testing including OIDC validation, RLS, and EF migrations.
/// </summary>
public class FullStackWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly KeycloakContainerFixture _keycloak;
    private readonly PostgresContainerFixture _postgres;

    public FullStackWebApplicationFactory(
        KeycloakContainerFixture keycloak,
        PostgresContainerFixture postgres)
    {
        _keycloak = keycloak;
        _postgres = postgres;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Replace DbContext with real PostgreSQL from Testcontainer
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_postgres.ConnectionString);
                options.UseSnakeCaseNamingConvention();
            });

            // Override JWT Bearer to use real Keycloak container
            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Authority = _keycloak.Authority;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters.ValidIssuer = _keycloak.Issuer;
                });
        });
    }

    /// <summary>
    /// Creates an HttpClient with a real Keycloak access token.
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateClient();
        var token = await _keycloak.GetAccessTokenAsync();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
