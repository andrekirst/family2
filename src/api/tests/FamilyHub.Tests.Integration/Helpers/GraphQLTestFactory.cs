using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Mutations;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Custom WebApplicationFactory for GraphQL tests with proper authentication mocking.
/// Ensures GraphQL schema is properly initialized before mocking services.
/// </summary>
public sealed class GraphQLTestFactory : WebApplicationFactory<Program>
{
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly PostgreSqlContainerFixture _containerFixture;

    public GraphQLTestFactory(PostgreSqlContainerFixture containerFixture)
    {
        _containerFixture = containerFixture;
        // Create a single mock service that tests can configure
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
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
        _mockCurrentUserService.GetUserId().Returns(_ => throw new UnauthorizedAccessException("User is not authenticated."));
        _mockCurrentUserService.GetUserEmail().Returns((Email?)null);
        _mockCurrentUserService.IsAuthenticated.Returns(false);
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registrations
            services.RemoveAll<DbContextOptions<AuthDbContext>>();
            services.RemoveAll<AuthDbContext>();

            // Add AuthDbContext with test container connection string
            services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(_containerFixture.ConnectionString)
                    .UseSnakeCaseNamingConvention());

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

            // Build service provider and apply migrations
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            dbContext.Database.Migrate();
        });
    }
}
