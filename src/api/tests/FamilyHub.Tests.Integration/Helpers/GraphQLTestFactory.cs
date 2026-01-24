using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Infrastructure.Authorization;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Custom WebApplicationFactory for GraphQL tests with proper authentication mocking.
/// Ensures GraphQL schema is properly initialized before mocking services.
/// </summary>
public sealed class GraphQlTestFactory(PostgreSqlContainerFixture containerFixture) : WebApplicationFactory<Program>
{
    private readonly ICurrentUserService _mockCurrentUserService = Substitute.For<ICurrentUserService>();

    // Store external user ID for claims
    private string? _externalUserId;

    // Create a single mock service that tests can configure

    /// <summary>
    /// Configures the authenticated user for the current test.
    /// Must be called before creating the HTTP client.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userId">User's internal ID</param>
    /// <param name="externalUserId">User's external provider ID (optional, defaults to userId)</param>
    public void SetAuthenticatedUser(Email email, UserId userId, string? externalUserId = null)
    {
        // Clear any previous configuration (reset the mock)
        _mockCurrentUserService.ClearReceivedCalls();

        // Configure the mock
        _mockCurrentUserService.GetUserId().Returns(userId);
        _mockCurrentUserService.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(userId);
        _mockCurrentUserService.GetUserEmail().Returns(email);
        _mockCurrentUserService.IsAuthenticated.Returns(true);

        // Store external ID for JWT claims (default to userId if not provided)
        _externalUserId = externalUserId ?? userId.Value.ToString();
    }

    /// <summary>
    /// Gets the external user ID for JWT claims.
    /// </summary>
    public string? GetExternalUserId() => _externalUserId;

    /// <summary>
    /// Clears the authenticated user (simulates unauthenticated request).
    /// </summary>
    public void ClearAuthenticatedUser()
    {
        _mockCurrentUserService.GetUserId().Returns(_ => throw new UnauthorizedAccessException("User is not authenticated."));
        _mockCurrentUserService.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns<UserId>(_ => throw new UnauthorizedAccessException("User is not authenticated."));
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
            services.RemoveAll<IDbContextFactory<AuthDbContext>>();
            // Note: IDbContextPool is internal - EF Core will handle pool cleanup automatically

            // Add AuthDbContext with test container connection string
            // Use AddPooledDbContextFactory to match production setup (singleton-safe)
            // CRITICAL: Must specify MigrationsAssembly or EF Core won't find migration files
            services.AddPooledDbContextFactory<AuthDbContext>((_, options) =>
                options.UseNpgsql(containerFixture.ConnectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(AuthDbContext).Assembly.GetName().Name);
                    })
                    .UseSnakeCaseNamingConvention());

            // Also register scoped DbContext for UnitOfWork pattern (same as production)
            services.AddScoped(sp =>
            {
                var factory = sp.GetRequiredService<IDbContextFactory<AuthDbContext>>();
                return factory.CreateDbContext();
            });

            // Remove existing FamilyDbContext registrations
            services.RemoveAll<DbContextOptions<FamilyDbContext>>();
            services.RemoveAll<FamilyDbContext>();
            services.RemoveAll<IDbContextFactory<FamilyDbContext>>();

            // Add FamilyDbContext with test container connection string
            // Changed from AddPooledDbContextFactory to AddDbContext to support IMediator and ILogger injection
            services.AddDbContext<FamilyDbContext>((sp, options) =>
                options.UseNpgsql(containerFixture.ConnectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(FamilyDbContext).Assembly.GetName().Name);
                    })
                    .UseSnakeCaseNamingConvention());

            // GraphQL types are auto-discovered via Program.cs → AddAuthModuleGraphQlTypes()
            // No explicit type registration needed here - WebApplicationFactory<Program> inherits the full DI container

            // Replace ICurrentUserService with our mock
            var currentUserServiceDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(ICurrentUserService));
            if (currentUserServiceDescriptor != null)
            {
                services.Remove(currentUserServiceDescriptor);
            }

            // Add the mock service (shared across all requests)
            services.AddScoped<ICurrentUserService>(_ => _mockCurrentUserService);

            // Remove real authorization handler and replace with test handler
            var authHandlerDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IAuthorizationHandler) &&
                d.ImplementationType == typeof(RequireOwnerOrAdminHandler));
            if (authHandlerDescriptor != null)
            {
                services.Remove(authHandlerDescriptor);
            }

            // Add test authorization handler that always succeeds
            services.AddScoped<IAuthorizationHandler, TestAuthorizationHandler>();

            // Register a hosted service that applies migrations on startup
            // NOTE: Using EnsureCreatedAsync() instead of Migrate() to avoid
            // PendingModelChangesWarning in .NET 10. This matches TestApplicationFactory behavior.
            services.AddHostedService<GraphQlMigrationHostedService>();
        });
    }
}

/// <summary>
/// Mock authorization handler that always succeeds for testing.
/// Bypasses JWT claim validation.
/// </summary>
internal sealed class TestAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        // Succeed all pending requirements
        foreach (var requirement in context.PendingRequirements.ToList())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Hosted service that applies EF Core database schemas on startup for GraphQL tests.
/// Creates both Auth and Family schemas for cross-module testing.
/// Uses idempotent schema creation to support shared database containers across tests.
/// </summary>
internal sealed class GraphQlMigrationHostedService(IServiceProvider serviceProvider) : IHostedService
{
    // Static flag to ensure schemas are only created once per test run
    // This is necessary because multiple tests share the same database container
    private static bool _schemasCreated;
    private static readonly object Lock = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Quick check without lock for performance
        if (_schemasCreated)
        {
            return;
        }

        lock (Lock)
        {
            // Double-check after acquiring lock
            if (_schemasCreated)
            {
                return;
            }

            _schemasCreated = true;
        }

        using var scope = serviceProvider.CreateScope();

        // Create Auth schema first
        var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await authDbContext.Database.EnsureCreatedAsync(cancellationToken);

        // Create Family schema using SQL script
        // Catch "already exists" error in case PostgreSqlContainerFixture already created it
        var familyDbContext = scope.ServiceProvider.GetRequiredService<FamilyDbContext>();
        try
        {
            var script = familyDbContext.Database.GenerateCreateScript();
            await familyDbContext.Database.ExecuteSqlRawAsync(script, cancellationToken);
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07") // 42P07 = duplicate_table
        {
            // Schema already exists - this is expected when PostgreSqlContainerFixture runs first
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
