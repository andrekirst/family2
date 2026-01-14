using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace FamilyHub.Infrastructure.Messaging;

/// <summary>
/// Health check for Redis connectivity.
/// </summary>
public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisHealthCheck"/> class.
    /// </summary>
    /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
    public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            await database.PingAsync();

            return HealthCheckResult.Healthy("Redis is responsive.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Redis is not responsive.",
                exception: ex);
        }
    }
}
