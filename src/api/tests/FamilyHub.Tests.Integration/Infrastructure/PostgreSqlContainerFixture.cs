using DotNet.Testcontainers.Builders;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Family.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Shared PostgreSQL container fixture for integration tests.
/// Each test class gets a fresh container with both Auth and Family schemas applied.
/// Uses Testcontainers to provide a real PostgreSQL 16 instance with automatic cleanup.
/// </summary>
/// <remarks>
/// PHASE 5 STATE: Creates both Auth schema (auth.*) and Family schema (family.*)
/// to support tests that require entities from both modules (e.g., TimestampInterceptorTests).
/// </remarks>
public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    /// <summary>
    /// Gets the PostgreSQL connection string for the running container.
    /// </summary>
    public string ConnectionString => _container?.GetConnectionString()
        ?? throw new InvalidOperationException("Container not started");

    /// <summary>
    /// Starts the PostgreSQL container and applies both Auth and Family schemas.
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

        // Apply both schemas (Auth first, then Family)
        await ApplyAuthSchemaAsync();
        await ApplyFamilySchemaAsync();
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
    /// Applies the Auth schema (auth.*) to the test database.
    /// Uses EnsureCreatedAsync() to create schema based on current model.
    /// </summary>
    private async Task ApplyAuthSchemaAsync()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var dbContext = new AuthDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Applies the Family schema (family.*) to the test database.
    /// Uses SQL script approach because EnsureCreatedAsync() won't work
    /// after AuthDbContext has already marked the database as "created".
    /// </summary>
    private async Task ApplyFamilySchemaAsync()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var dbContext = new FamilyDbContext(options);

        // Generate and execute creation script directly
        // This works even after EnsureCreated() thinks the database exists
        var script = dbContext.Database.GenerateCreateScript();
        await dbContext.Database.ExecuteSqlRawAsync(script);
    }
}
