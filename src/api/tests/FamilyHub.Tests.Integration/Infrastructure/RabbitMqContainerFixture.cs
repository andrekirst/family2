using DotNet.Testcontainers.Builders;
using Testcontainers.RabbitMq;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Shared RabbitMQ container fixture for integration tests.
/// Provides a real RabbitMQ instance with automatic cleanup.
/// Uses Testcontainers for ephemeral, isolated test environments.
/// </summary>
public sealed class RabbitMqContainerFixture : IAsyncLifetime
{
    private RabbitMqContainer? _container;

    /// <summary>
    /// Gets the RabbitMQ hostname for the running container.
    /// </summary>
    public string Host => _container?.Hostname
        ?? throw new InvalidOperationException("Container not started");

    /// <summary>
    /// Gets the RabbitMQ AMQP port for the running container.
    /// </summary>
    public int Port => _container?.GetMappedPublicPort(5672)
        ?? throw new InvalidOperationException("Container not started");

    /// <summary>
    /// Gets the RabbitMQ management UI port for the running container.
    /// </summary>
    public int ManagementPort => _container?.GetMappedPublicPort(15672)
        ?? throw new InvalidOperationException("Container not started");

    /// <summary>
    /// Gets the username for RabbitMQ authentication.
    /// </summary>
    public string Username => "guest";

    /// <summary>
    /// Gets the password for RabbitMQ authentication.
    /// </summary>
    public string Password => "guest";

    /// <summary>
    /// Gets the AMQP connection string for the running container.
    /// </summary>
    public string ConnectionString => _container?.GetConnectionString()
        ?? throw new InvalidOperationException("Container not started");

    /// <summary>
    /// Starts the RabbitMQ container with management plugin enabled.
    /// Called once per test collection before any tests run.
    /// </summary>
    public async Task InitializeAsync()
    {
        _container = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.13-management-alpine")
            .WithUsername(Username)
            .WithPassword(Password)
            .WithCleanUp(true) // Auto-cleanup via Ryuk sidecar
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("Server startup complete"))
            .Build();

        // Start RabbitMQ container (first run downloads image)
        await _container.StartAsync();
    }

    /// <summary>
    /// Stops the RabbitMQ container and cleans up resources.
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
