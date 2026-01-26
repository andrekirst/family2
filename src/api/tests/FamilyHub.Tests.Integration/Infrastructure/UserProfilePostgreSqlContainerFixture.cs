using DotNet.Testcontainers.Builders;
using FamilyHub.Modules.UserProfile.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Shared PostgreSQL container fixture for UserProfile module integration tests.
/// Each test class gets a fresh container with applied UserProfileDbContext migrations.
/// Uses Testcontainers to provide a real PostgreSQL 16 instance with automatic cleanup.
/// </summary>
public sealed class UserProfilePostgreSqlContainerFixture : IAsyncLifetime
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
            .WithDatabase("familyhub_userprofile_test")
            .WithUsername("test_user")
            .WithPassword(Guid.NewGuid().ToString()) // Random password per test run
            .WithCleanUp(true) // Auto-cleanup via Ryuk sidecar
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted("pg_isready"))
            .Build();

        // Start PostgreSQL container
        await _container.StartAsync();

        // Apply UserProfileDbContext schema
        await ApplyUserProfileDbContextSchemaAsync();
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
    /// Applies the UserProfileDbContext schema to the test database.
    /// Uses EnsureCreated() to create schema based on current model.
    /// </summary>
    private async Task ApplyUserProfileDbContextSchemaAsync()
    {
        var services = new ServiceCollection();

        services.AddDbContext<UserProfileDbContext>(options =>
            options.UseNpgsql(ConnectionString)
                .UseSnakeCaseNamingConvention());

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<UserProfileDbContext>();

        // Use EnsureCreated() to create schema based on UserProfileDbContext model
        await dbContext.Database.EnsureCreatedAsync();
    }
}
