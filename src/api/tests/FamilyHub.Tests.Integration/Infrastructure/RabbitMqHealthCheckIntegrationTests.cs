using FamilyHub.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for RabbitMqHealthCheck using real RabbitMQ.
/// Tests actual health check behavior with a running RabbitMQ instance.
/// </summary>
[Collection("RabbitMQ")]
public sealed class RabbitMqHealthCheckIntegrationTests(RabbitMqContainerFixture fixture)
{
    private readonly ILogger<RabbitMqHealthCheck> _logger = Substitute.For<ILogger<RabbitMqHealthCheck>>();

    [Fact]
    public async Task CheckHealthAsync_WithRunningRabbitMq_ReturnsHealthy()
    {
        // Arrange
        var settings = new RabbitMqSettings
        {
            Host = fixture.Host,
            Port = fixture.Port,
            Username = fixture.Username,
            Password = fixture.Password
        };
        var healthCheck = new RabbitMqHealthCheck(_logger, Options.Create(settings));
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CheckHealthAsync_WithRunningRabbitMq_ReturnsServerInfo()
    {
        // Arrange
        var settings = new RabbitMqSettings
        {
            Host = fixture.Host,
            Port = fixture.Port,
            Username = fixture.Username,
            Password = fixture.Password
        };
        var healthCheck = new RabbitMqHealthCheck(_logger, Options.Create(settings));
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data.Should().NotBeEmpty();
        result.Data.Should().ContainKey("serverProduct");
        result.Data.Should().ContainKey("serverVersion");
        result.Data["serverProduct"].Should().Be("RabbitMQ");
    }

    [Fact]
    public async Task CheckHealthAsync_WithInvalidHost_ReturnsUnhealthy()
    {
        // Arrange
        var settings = new RabbitMqSettings
        {
            Host = "nonexistent.host.local",
            Port = 5672,
            Username = "guest",
            Password = "guest"
        };
        var healthCheck = new RabbitMqHealthCheck(_logger, Options.Create(settings));
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckHealthAsync_WithWrongCredentials_ReturnsUnhealthy()
    {
        // Arrange
        var settings = new RabbitMqSettings
        {
            Host = fixture.Host,
            Port = fixture.Port,
            Username = "wrong_user",
            Password = "wrong_password"
        };
        var healthCheck = new RabbitMqHealthCheck(_logger, Options.Create(settings));
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
    }
}
