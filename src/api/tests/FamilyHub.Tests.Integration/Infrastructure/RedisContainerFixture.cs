using DotNet.Testcontainers.Builders;
using Testcontainers.Redis;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Shared Redis container fixture for integration tests.
/// Provides a real Redis instance with automatic cleanup.
/// Uses Testcontainers for ephemeral, isolated test environments.
/// </summary>
public sealed class RedisContainerFixture : IAsyncLifetime
{
    private RedisContainer? _container;

    /// <summary>
    /// Gets the Redis hostname for the running container.
    /// </summary>
    public string Host => _container?.Hostname
        ?? throw new InvalidOperationException("Container not started");

    /// <summary>
    /// Gets the Redis port for the running container.
    /// </summary>
    public int Port => _container?.GetMappedPublicPort(6379)
        ?? throw new InvalidOperationException("Container not started");

    /// <summary>
    /// Gets the Redis connection string for the running container.
    /// </summary>
    public string ConnectionString => _container?.GetConnectionString()
        ?? throw new InvalidOperationException("Container not started");

    /// <summary>
    /// Starts the Redis container.
    /// Called once per test collection before any tests run.
    /// </summary>
    public async Task InitializeAsync()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true) // Auto-cleanup via Ryuk sidecar
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted("redis-cli", "ping"))
            .Build();

        // Start Redis container (first run downloads image, ~10-20s cached)
        await _container.StartAsync();
    }

    /// <summary>
    /// Stops the Redis container and cleans up resources.
    /// Called once per test collection after all tests complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }
}
