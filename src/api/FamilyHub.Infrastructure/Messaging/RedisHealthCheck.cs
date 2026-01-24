using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace FamilyHub.Infrastructure.Messaging;

/// <summary>
/// Health check for Redis connectivity.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RedisHealthCheck"/> class.
/// </remarks>
/// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
public sealed class RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer) : IHealthCheck
{
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;

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
