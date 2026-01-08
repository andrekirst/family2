using DotNet.Testcontainers.Builders;
using FamilyHub.Modules.Auth.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Shared PostgreSQL container fixture for integration tests.
/// Each test class gets a fresh container with applied migrations.
/// Uses Testcontainers to provide a real PostgreSQL 16 instance with automatic cleanup.
/// </summary>
public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    /// <summary>
    /// Gets the PostgreSQL connection string for the running container.
    /// </summary>
    public string ConnectionString => _container?.GetConnectionString()
        ?? throw new InvalidOperationException("Container not started");

    /// <summary>
    /// Starts the PostgreSQL container and applies EF Core migrations.
    /// Called once per test collection before any tests run.
    /// </summary>
    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("familyhub_test")
            .WithUsername("test_user")
            .WithPassword(Guid.NewGuid().ToString()) // Random password per test run
            .WithCleanUp(true) // Auto-cleanup via Ryuk sidecar
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted("pg_isready"))
            .Build();

        // Start PostgreSQL container (60-80s first run, 10-20s cached)
        await _container.StartAsync();

        // Apply EF Core migrations to create schema
        await ApplyMigrationsAsync();
    }

    /// <summary>
    /// Stops the PostgreSQL container and cleans up resources.
    /// Called once per test collection after all tests complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Applies all pending EF Core migrations to the test database.
    /// Uses production configuration to avoid assembly loading issues.
    /// </summary>
    private async Task ApplyMigrationsAsync()
    {
        // Create a temporary service provider with test database configuration
        var services = new ServiceCollection();

        // Configure DbContext with migrations
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(ConnectionString)
                .UseSnakeCaseNamingConvention());

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        // Apply migrations (production-like behavior)
        // This ensures we test the actual migration files that will run in production
        await dbContext.Database.MigrateAsync();
    }
}
