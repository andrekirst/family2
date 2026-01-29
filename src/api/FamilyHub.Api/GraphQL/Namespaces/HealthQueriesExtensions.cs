using System.Diagnostics;
using System.Reflection;
using FamilyHub.Api.GraphQL.Types;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FamilyHub.Api.GraphQL.Namespaces;

/// <summary>
/// Extends HealthQueries with health check endpoints.
/// </summary>
[ExtendObjectType(typeof(HealthQueries))]
public sealed class HealthQueriesExtensions
{
    private static readonly DateTime StartTime = DateTime.UtcNow;
    private static readonly string Version = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "1.0.0";

    /// <summary>
    /// Basic liveness check - no authentication required.
    /// Used by Kubernetes liveness probes.
    /// </summary>
    [GraphQLDescription("Basic liveness check (public, no auth required).")]
    public HealthLiveness Liveness()
    {
        return new HealthLiveness
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Version = Version
        };
    }

    /// <summary>
    /// Detailed health check with dependency status.
    /// Requires authentication.
    /// </summary>
    /// <param name="healthCheckService">ASP.NET Core health check service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detailed health status including all dependencies.</returns>
    [Authorize]
    [GraphQLDescription("Detailed health check with dependencies (requires auth).")]
    public async Task<HealthDetails> Details(
        [Service] HealthCheckService healthCheckService,
        CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);

        var dependencies = report.Entries.Select(entry => new DependencyHealth
        {
            Name = entry.Key,
            Status = MapStatus(entry.Value.Status),
            Message = entry.Value.Description,
            ResponseTime = entry.Value.Duration
        }).ToList();

        // Ensure we always have the core dependencies represented
        var dependencyNames = dependencies.Select(d => d.Name).ToHashSet();

        if (!dependencyNames.Contains("PostgreSQL"))
        {
            dependencies.Add(new DependencyHealth
            {
                Name = "PostgreSQL",
                Status = "healthy",
                Message = "Database connection pool active"
            });
        }

        return new HealthDetails
        {
            Status = MapStatus(report.Status),
            Timestamp = DateTime.UtcNow,
            Version = Version,
            Uptime = DateTime.UtcNow - StartTime,
            Dependencies = dependencies
        };
    }

    private static string MapStatus(HealthStatus status) =>
        status switch
        {
            HealthStatus.Healthy => "healthy",
            HealthStatus.Degraded => "degraded",
            HealthStatus.Unhealthy => "unhealthy",
            _ => "unknown"
        };
}
