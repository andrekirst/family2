using Testcontainers.PostgreSql;

namespace FamilyHub.IntegrationTests.Fixtures;

/// <summary>
/// Starts a real PostgreSQL 16 container for E2E tests that require
/// PostgreSQL-specific features (RLS, PL/pgSQL, raw SQL).
/// </summary>
public class PostgresContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("familyhub_test")
            .WithUsername("familyhub")
            .WithPassword("familyhub_test")
            .Build();

        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
