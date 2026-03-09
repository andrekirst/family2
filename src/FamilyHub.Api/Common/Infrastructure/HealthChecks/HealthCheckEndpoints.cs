using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace FamilyHub.Api.Common.Infrastructure.HealthChecks;

/// <summary>
/// Maps health check endpoints with liveness/readiness split for container orchestrators.
/// - /health/live  -> Kubernetes livenessProbe (is the process alive?)
/// - /health/ready -> Kubernetes readinessProbe (can it serve traffic?)
/// - /health       -> All checks combined (operational dashboard)
/// </summary>
public static class HealthCheckEndpoints
{
    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // Liveness probe: only "live" tagged checks (lightweight self-check).
        // If this fails, Kubernetes should restart the pod.
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        // Readiness probe: only "ready" tagged checks (database, Keycloak, GraphQL).
        // If this fails, Kubernetes removes the pod from the service load balancer.
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        // Combined endpoint: runs all checks regardless of tags.
        // Used by monitoring dashboards for a full system overview.
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        return app;
    }
}
