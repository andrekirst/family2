using FamilyHub.SharedKernel.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FamilyHub.Infrastructure.Messaging;

/// <summary>
/// Extension methods for registering RabbitMQ services in the DI container.
/// </summary>
public static class RabbitMqServiceExtensions
{
    /// <summary>
    /// Adds RabbitMQ services including publisher and health check.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>Registers the following services:</para>
    /// <list type="bullet">
    /// <item><see cref="RabbitMqSettings"/> - Configuration options from "RabbitMQ" section</item>
    /// <item><see cref="IMessageBrokerPublisher"/> - Singleton publisher (manages connection pooling)</item>
    /// <item><see cref="RabbitMqHealthCheck"/> - Singleton health check for RabbitMQ connectivity</item>
    /// </list>
    /// <para>
    /// Configuration example in appsettings.json:
    /// <code>
    /// {
    ///   "RabbitMQ": {
    ///     "Host": "localhost",
    ///     "Port": 5672,
    ///     "Username": "familyhub",
    ///     "Password": "secret"
    ///   }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration from "RabbitMQ" section
        services.Configure<RabbitMqSettings>(
            configuration.GetSection(RabbitMqSettings.SectionName));

        // Register publisher as singleton (manages its own connection lifecycle)
        services.AddSingleton<IMessageBrokerPublisher, RabbitMqPublisher>();

        // Register health check as singleton
        services.AddSingleton<RabbitMqHealthCheck>();

        return services;
    }

    /// <summary>
    /// Adds the RabbitMQ health check to the health checks builder.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The health check name. Default: "rabbitmq".</param>
    /// <param name="failureStatus">The failure status. Default: <see cref="HealthStatus.Unhealthy"/>.</param>
    /// <param name="tags">Optional tags for the health check (e.g., "ready", "infrastructure").</param>
    /// <returns>The health checks builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Call this method after <see cref="AddRabbitMq"/> to include RabbitMQ
    /// in the health check system.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// builder.Services.AddRabbitMq(builder.Configuration);
    /// builder.Services.AddHealthChecks()
    ///     .AddRabbitMqHealthCheck("rabbitmq", tags: ["ready", "infrastructure"]);
    /// </code>
    /// </para>
    /// </remarks>
    public static IHealthChecksBuilder AddRabbitMqHealthCheck(
        this IHealthChecksBuilder builder,
        string name = "rabbitmq",
        HealthStatus? failureStatus = null,
        params string[] tags)
    {
        return builder.AddCheck<RabbitMqHealthCheck>(
            name,
            failureStatus: failureStatus ?? HealthStatus.Unhealthy,
            tags: tags);
    }
}
