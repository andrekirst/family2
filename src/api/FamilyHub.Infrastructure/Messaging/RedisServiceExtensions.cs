using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FamilyHub.Infrastructure.Messaging;

/// <summary>
/// Extension methods for registering Redis services in the DI container.
/// </summary>
public static class RedisServiceExtensions
{
    /// <summary>
    /// Registers Redis connection and PubSub for GraphQL subscriptions.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>Registers the following services:</para>
    /// <list type="bullet">
    /// <item><see cref="RedisSettings"/> - Configuration options from "Redis" section</item>
    /// <item><see cref="IConnectionMultiplexer"/> - Singleton Redis connection (thread-safe, reusable)</item>
    /// <item><see cref="IDatabase"/> - Scoped Redis database instance</item>
    /// </list>
    /// <para>
    /// Configuration example in appsettings.json:
    /// <code>
    /// {
    ///   "Redis": {
    ///     "ConnectionString": "localhost:6379",
    ///     "InstanceName": "FamilyHub:",
    ///     "Channels": {
    ///       "FamilyMembers": "family-members-changed",
    ///       "PendingInvitations": "pending-invitations-changed"
    ///     }
    ///   }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration from "Redis" section
        services.Configure<RedisSettings>(
            configuration.GetSection(RedisSettings.SectionName));

        // Register Redis connection multiplexer as singleton (thread-safe)
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
            var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();

            if (!settings.IsValid())
            {
                throw new InvalidOperationException(
                    "Redis settings are incomplete. Please check appsettings.json.");
            }

            var configurationOptions = ConfigurationOptions.Parse(settings.ConnectionString);
            configurationOptions.AbortOnConnectFail = settings.AbortOnConnectFail;
            configurationOptions.ConnectTimeout = (int)settings.ConnectionTimeout.TotalMilliseconds;
            configurationOptions.ClientName = $"{settings.InstanceName}Api";

            logger.LogInformation(
                "Connecting to Redis at {ConnectionString}",
                settings.ConnectionString);

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        // Register Redis database (scoped for per-request usage)
        services.AddScoped<IDatabase>(sp =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            return multiplexer.GetDatabase();
        });

        // Register subscription publisher for GraphQL real-time updates
        services.AddScoped<IRedisSubscriptionPublisher, RedisSubscriptionPublisher>();

        // Register Redis health check
        services.AddSingleton<RedisHealthCheck>();

        // Note: SubscriptionEventPublisher is registered in Auth module
        // (Auth-specific types can't be in Infrastructure layer)

        return services;
    }

    /// <summary>
    /// Adds the Redis health check to the health checks builder.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="tags">Optional tags for the health check (e.g., "ready", "infrastructure").</param>
    /// <returns>The health checks builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Call this method after <see cref="AddRedis"/> to include Redis
    /// in the health check system. Uses the registered IConnectionMultiplexer
    /// to check Redis connectivity.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// builder.Services.AddRedis(builder.Configuration);
    /// builder.Services.AddHealthChecks()
    ///     .AddRedisHealthCheck(tags: ["ready", "infrastructure"]);
    /// </code>
    /// </para>
    /// </remarks>
    public static IHealthChecksBuilder AddRedisHealthCheck(
        this IHealthChecksBuilder builder,
        params string[] tags)
    {
        return builder.AddCheck<RedisHealthCheck>(
            "redis",
            failureStatus: HealthStatus.Unhealthy,
            tags: tags);
    }
}
