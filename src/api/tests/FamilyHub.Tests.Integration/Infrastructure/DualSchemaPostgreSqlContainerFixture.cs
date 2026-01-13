using DotNet.Testcontainers.Builders;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Family.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Shared PostgreSQL container fixture that initializes both Auth and Family schemas.
/// Used for cross-DbContext consistency testing where entities reference IDs across schemas.
/// Both AuthDbContext (auth schema) and FamilyDbContext (family schema) are configured.
/// </summary>
/// <remarks>
/// This fixture is necessary because Family Hub uses a modular monolith architecture
/// where each module has its own schema and DbContext, but entities can reference
/// IDs across schemas (e.g., User.FamilyId references family.families.id).
/// No foreign key constraints exist between schemas (by design for DDD bounded contexts).
/// </remarks>
public sealed class DualSchemaPostgreSqlContainerFixture : IAsyncLifetime
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
            .WithDatabase("familyhub_dual_schema_test")
            .WithUsername("test_user")
            .WithPassword(Guid.NewGuid().ToString()) // Random password per test run
            .WithCleanUp(true) // Auto-cleanup via Ryuk sidecar
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted("pg_isready"))
            .Build();

        // Start PostgreSQL container (60-80s first run, 10-20s cached)
        await _container.StartAsync();

        // Apply both schemas in correct order (Auth first, then Family)
        // Order matters if there were FK constraints, though we don't have any
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
    /// Creates users and outbox_events tables.
    /// </summary>
    private async Task ApplyAuthSchemaAsync()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var dbContext = new AuthDbContext(options);

        // Use Database.EnsureCreatedAsync() to create schema based on AuthDbContext model
        await dbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Applies the Family schema (family.*) to the test database.
    /// Creates families and family_member_invitations tables.
    /// </summary>
    /// <remarks>
    /// Since AuthDbContext.EnsureCreated() may mark the database as "created",
    /// we use raw SQL to ensure the family schema gets created properly.
    /// </remarks>
    private async Task ApplyFamilySchemaAsync()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var dbContext = new FamilyDbContext(options);

        // Get the model's relational model to generate creation script
        var script = dbContext.Database.GenerateCreateScript();

        // Execute the creation script directly to ensure family schema is created
        // This works even if EnsureCreated() thinks the database already exists
        await dbContext.Database.ExecuteSqlRawAsync(script);
    }

    /// <summary>
    /// Creates a new AuthDbContext instance for verification queries.
    /// Use this to create fresh contexts that don't share change tracker state.
    /// </summary>
    public AuthDbContext CreateAuthDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AuthDbContext(options);
    }

    /// <summary>
    /// Creates a new FamilyDbContext instance for verification queries.
    /// Use this to create fresh contexts that don't share change tracker state.
    /// </summary>
    public FamilyDbContext CreateFamilyDbContext()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new FamilyDbContext(options);
    }
}
