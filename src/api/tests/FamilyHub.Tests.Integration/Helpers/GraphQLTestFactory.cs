using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Mutations;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Custom WebApplicationFactory for GraphQL tests with proper authentication mocking.
/// Ensures GraphQL schema is properly initialized before mocking services.
/// </summary>
public sealed class GraphQLTestFactory : WebApplicationFactory<Program>
{
    private readonly ICurrentUserService _mockCurrentUserService;

    public GraphQLTestFactory()
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

        // Create a single mock service that tests can configure
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();

        // Force server/host creation to trigger migrations in CreateHost()
        // This ensures database is ready before tests access Services property
        _ = Server;
    }

    /// <summary>
    /// Configures the authenticated user for the current test.
    /// Must be called before creating the HTTP client.
    /// </summary>
    public void SetAuthenticatedUser(Email email, UserId userId)
    {
        _mockCurrentUserService.GetUserId().Returns(userId);
        _mockCurrentUserService.GetUserEmail().Returns(email);
        _mockCurrentUserService.IsAuthenticated.Returns(true);
    }

    /// <summary>
    /// Clears the authenticated user (simulates unauthenticated request).
    /// </summary>
    public void ClearAuthenticatedUser()
    {
        _mockCurrentUserService.GetUserId().Returns((UserId?)null);
        _mockCurrentUserService.GetUserEmail().Returns((Email?)null);
        _mockCurrentUserService.IsAuthenticated.Returns(false);
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        // Add test Zitadel configuration to prevent startup errors in CI
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testZitadelSettings = new Dictionary<string, string?>
            {
                ["Zitadel:Authority"] = "https://test.zitadel.cloud",
                ["Zitadel:ClientId"] = "test-client-id",
                ["Zitadel:ClientSecret"] = "test-client-secret",
                ["Zitadel:RedirectUri"] = "https://localhost:5001/callback",
                ["Zitadel:Scope"] = "openid profile email",
                ["Zitadel:Audience"] = "test-client-id"  // Required by ZitadelSettings.IsValid()
            };

            config.AddInMemoryCollection(testZitadelSettings);
        });

        builder.ConfigureServices(services =>
        {
            // Explicitly ensure GraphQL types are registered
            // This is a workaround for the type discovery not working in test environment
            services.AddGraphQLServer()
                .AddTypeExtension<FamilyMutations>()
                .AddTypeExtension<AuthMutations>()
                .AddTypeExtension<AuthQueries>()
                .AddTypeExtension<HealthQueries>()
                .AddTypeExtension<UserQueries>();

            // Replace ICurrentUserService with our mock
            var currentUserServiceDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(ICurrentUserService));
            if (currentUserServiceDescriptor != null)
            {
                services.Remove(currentUserServiceDescriptor);
            }

            // Add the mock service (shared across all requests)
            services.AddScoped<ICurrentUserService>(_ => _mockCurrentUserService);
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
            // Apply migrations (EF Core migrations handle database AND schema creation)
            var logPath = "/tmp/graphql-factory-migrations.log";
            File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] GRAPHQL-FACTORY: Applying migrations to: {connectionString}\n");

            // CRITICAL: EnsureCreated() creates the DATABASE if it doesn't exist
            // Then Migrate() applies the schema migrations
            authDbContext.Database.EnsureCreated();
            authDbContext.Database.Migrate();

            File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] GRAPHQL-FACTORY: Migrations applied successfully\n");

            // Verify schema exists
            var canConnect = authDbContext.Database.CanConnect();
            File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] GRAPHQL-FACTORY: Can connect: {canConnect}\n");
        }
        catch (Exception ex)
        {
            var logPath = "/tmp/graphql-factory-migrations.log";
            File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] GRAPHQL-FACTORY: MIGRATION FAILED: {ex.Message}\n");
            File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] GRAPHQL-FACTORY: Stack trace: {ex.StackTrace}\n");
            throw;
        }

        return host;
    }
}
