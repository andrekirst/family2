using FamilyHub.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FamilyHub.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for RabbitMqHealthCheck.
/// Tests verify health check behavior with real and unreachable RabbitMQ instances.
/// </summary>
[Collection("RabbitMQ")]
public class RabbitMqHealthCheckTests
{
    private readonly RabbitMqContainerFixture _fixture;

    public RabbitMqHealthCheckTests(RabbitMqContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CheckHealthAsync_RabbitMqAvailable_ShouldReturnHealthy()
    {
        // Arrange
        var settings = CreateSettings(_fixture.Host, _fixture.Port);
        var healthCheck = CreateHealthCheck(settings);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "rabbitmq",
                healthCheck,
                failureStatus: HealthStatus.Unhealthy,
                tags: null)
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("RabbitMQ");
        result.Data.Should().ContainKey("host");
        result.Data.Should().ContainKey("port");
        result.Data["host"].Should().Be(_fixture.Host);
        result.Data["port"].Should().Be(_fixture.Port);
    }

    [Fact]
    public async Task CheckHealthAsync_RabbitMqAvailable_ShouldIncludeServerProperties()
    {
        // Arrange
        var settings = CreateSettings(_fixture.Host, _fixture.Port);
        var healthCheck = CreateHealthCheck(settings);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "rabbitmq",
                healthCheck,
                failureStatus: HealthStatus.Unhealthy,
                tags: null)
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data.Should().ContainKey("serverProduct");
        result.Data.Should().ContainKey("serverVersion");
        result.Data["serverProduct"].Should().Be("RabbitMQ");
    }

    [Fact]
    public async Task CheckHealthAsync_RabbitMqUnavailable_ShouldReturnUnhealthy()
    {
        // Arrange - Use an invalid port that won't have RabbitMQ
        var settings = CreateSettings("localhost", 59999);
        var healthCheck = CreateHealthCheck(settings);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "rabbitmq",
                healthCheck,
                failureStatus: HealthStatus.Unhealthy,
                tags: null)
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("failed");
        result.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckHealthAsync_InvalidHost_ShouldReturnUnhealthy()
    {
        // Arrange - Use a non-existent host
        var settings = CreateSettings("non-existent-host-xyz.invalid", 5672);
        var healthCheck = CreateHealthCheck(settings);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "rabbitmq",
                healthCheck,
                failureStatus: HealthStatus.Unhealthy,
                tags: null)
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckHealthAsync_WithCancellation_ShouldReturnUnhealthy()
    {
        // Arrange
        var settings = CreateSettings(_fixture.Host, _fixture.Port);
        var healthCheck = CreateHealthCheck(settings);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "rabbitmq",
                healthCheck,
                failureStatus: HealthStatus.Unhealthy,
                tags: null)
        };

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act - Health check catches exceptions and returns Unhealthy instead of throwing
        var result = await healthCheck.CheckHealthAsync(context, cts.Token);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckHealthAsync_MultipleCalls_ShouldAllReturnHealthy()
    {
        // Arrange
        var settings = CreateSettings(_fixture.Host, _fixture.Port);
        var healthCheck = CreateHealthCheck(settings);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "rabbitmq",
                healthCheck,
                failureStatus: HealthStatus.Unhealthy,
                tags: null)
        };

        // Act - Multiple health checks in sequence
        var results = new List<HealthCheckResult>();
        for (var i = 0; i < 5; i++)
        {
            results.Add(await healthCheck.CheckHealthAsync(context));
        }

        // Assert
        results.Should().AllSatisfy(r => r.Status.Should().Be(HealthStatus.Healthy));
    }

    private static RabbitMqSettings CreateSettings(string host, int port)
    {
        return new RabbitMqSettings
        {
            Host = host,
            Port = port,
            Username = "guest",
            Password = "guest",
            VirtualHost = "/",
            ConnectionTimeout = TimeSpan.FromSeconds(5)
        };
    }

    private static RabbitMqHealthCheck CreateHealthCheck(RabbitMqSettings settings)
    {
        var options = Options.Create(settings);
        var logger = Substitute.For<ILogger<RabbitMqHealthCheck>>();
        return new RabbitMqHealthCheck(logger, options);
    }
}
