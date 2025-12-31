using DotNet.Testcontainers.Builders;
using FamilyHub.Modules.Auth.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Shared PostgreSQL container fixture for integration tests.
/// Uses Testcontainers to provide a real PostgreSQL 16 instance with automatic cleanup.
/// Implements fast database reset via Respawn (50-200ms) instead of container restart (60s).
/// </summary>
public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("familyhub_test")
        .WithUsername("test_user")
        .WithPassword(Guid.NewGuid().ToString()) // Random password per test run
        .WithCleanUp(true) // Auto-cleanup via Ryuk sidecar
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilCommandIsCompleted("pg_isready"))
        .Build();
    private Respawner? _respawner;

    // Configure PostgreSQL 16 container with alpine variant (150MB vs 400MB)
    // Random password per test run
    // Auto-cleanup via Ryuk sidecar

    /// <summary>
    /// Gets the PostgreSQL connection string for the running container.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Starts the PostgreSQL container and applies EF Core migrations.
    /// Called once per test collection before any tests run.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Start PostgreSQL container (60-80s first run, 10-20s cached)
        await _container.StartAsync();

        // Apply EF Core migrations to create schema
        await ApplyMigrationsAsync();

        // Initialize Respawner for fast database cleanup
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            TablesToIgnore = ["__EFMigrationsHistory"], // Keep migration history
            SchemasToExclude = ["information_schema", "pg_catalog"], // Skip system schemas
            DbAdapter = DbAdapter.Postgres
        });
    }

    /// <summary>
    /// Resets the database to a clean state by truncating all tables.
    /// Much faster than restarting the container (50-200ms vs 60s).
    /// Call this at the start of each test to ensure isolation.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_respawner == null)
        {
            throw new InvalidOperationException("Respawner not initialized. Call InitializeAsync first.");
        }

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Stops the PostgreSQL container and cleans up resources.
    /// Called once per test collection after all tests complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Applies all pending EF Core migrations to the test database.
    /// Uses the AuthDbContext as the primary migration source.
    /// </summary>
    /// <summary>
    /// Applies all pending EF Core migrations to the test database.
    /// Uses the AuthDbContext as the primary migration source.
    /// </summary>
    private async Task ApplyMigrationsAsync()
    {
        Console.WriteLine("[TEST-FIXTURE] Starting ApplyMigrationsAsync");
        Console.WriteLine($"[TEST-FIXTURE] Connection string: {ConnectionString}");
        Console.WriteLine($"[TEST-FIXTURE] AuthDbContext assembly: {typeof(AuthDbContext).Assembly.GetName().Name}");
        Console.WriteLine($"[TEST-FIXTURE] Migrations assembly location: {typeof(AuthDbContext).Assembly.Location}");

        // Create a temporary service provider with test database configuration
        var services = new ServiceCollection();

        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(ConnectionString, npgsqlOptions =>
                {
                    // CRITICAL: Specify migrations assembly explicitly for test context
                    // EF Core auto-discovery doesn't work when DbContext is created outside the main application
                    var assemblyName = typeof(AuthDbContext).Assembly.GetName().Name;
                    Console.WriteLine($"[TEST-FIXTURE] Setting MigrationsAssembly to: {assemblyName}");
                    npgsqlOptions.MigrationsAssembly(assemblyName);
                })
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        Console.WriteLine("[TEST-FIXTURE] DbContext created, about to run MigrateAsync");
        // Apply all pending migrations
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("[TEST-FIXTURE] MigrateAsync completed");

        // Verify tables were created
        var tables = await dbContext.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'auth'");
        Console.WriteLine($"[TEST-FIXTURE] Tables in auth schema: {tables}");
    }
}
