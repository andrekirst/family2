using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Mutations;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Custom WebApplicationFactory for GraphQL tests with Testcontainers PostgreSQL support.
/// Provides authentication mocking via NSubstitute for integration tests.
/// </summary>
public sealed class GraphQlTestFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly ICurrentUserService _mockCurrentUserService;

    /// <summary>
    /// Creates a new GraphQL test factory with the specified PostgreSQL connection string.
    /// </summary>
    /// <param name="connectionString">Connection string from Testcontainers PostgreSQL fixture</param>
    public GraphQlTestFactory(string connectionString)
    {
        _connectionString = connectionString;

        // Set Zitadel OAuth configuration environment variables
        Environment.SetEnvironmentVariable("Zitadel__Authority", "https://test.zitadel.cloud");
        Environment.SetEnvironmentVariable("Zitadel__ClientId", "test-client-id");
        Environment.SetEnvironmentVariable("Zitadel__ClientSecret", "test-client-secret");
        Environment.SetEnvironmentVariable("Zitadel__RedirectUri", "https://localhost:5001/callback");
        Environment.SetEnvironmentVariable("Zitadel__Scope", "openid profile email");
        Environment.SetEnvironmentVariable("Zitadel__Audience", "test-client-id");

        // Create mock service for authentication
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();

        // Force server creation (triggers configuration)
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
        _mockCurrentUserService.GetUserId().Returns(_ => throw new UnauthorizedAccessException("User is not authenticated"));
        _mockCurrentUserService.GetUserEmail().Returns((Email?)null);
        _mockCurrentUserService.IsAuthenticated.Returns(false);
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        // Override database connection string with Testcontainers instance
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:FamilyHubDb"] = _connectionString,
                ["Zitadel:Authority"] = "https://test.zitadel.cloud",
                ["Zitadel:ClientId"] = "test-client-id",
                ["Zitadel:ClientSecret"] = "test-client-secret",
                ["Zitadel:RedirectUri"] = "https://localhost:5001/callback",
                ["Zitadel:Scope"] = "openid profile email",
                ["Zitadel:Audience"] = "test-client-id"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Explicitly ensure GraphQL types are registered
            services.AddGraphQLServer()
                .AddTypeExtension<FamilyMutations>()
                .AddTypeExtension<AuthMutations>()
                .AddTypeExtension<AuthQueries>()
                .AddTypeExtension<HealthQueries>()
                .AddTypeExtension<UserQueries>();

            // Replace ICurrentUserService with mock for authentication testing
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
}
