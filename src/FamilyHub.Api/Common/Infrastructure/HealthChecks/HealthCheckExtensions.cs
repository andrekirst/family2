using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FamilyHub.Api.Common.Infrastructure.HealthChecks;

/// <summary>
/// Registers all application health checks with liveness/readiness tags.
/// Liveness ("live"): indicates the process is running and not deadlocked.
/// Readiness ("ready"): indicates the service can accept traffic (dependencies are reachable).
/// </summary>
public static class HealthCheckExtensions
{
    public static readonly string[] LiveTag = ["live"];
    public static readonly string[] ReadyTag = ["ready"];

    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=familyhub;Username=familyhub;Password=familyhub";

        services.AddHealthChecks()
            // Liveness: lightweight self-check proving the process is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: LiveTag)

            // Readiness: PostgreSQL database connectivity
            .AddNpgSql(
                connectionString,
                name: "postgresql",
                healthQuery: "SELECT 1;",
                failureStatus: HealthStatus.Unhealthy,
                tags: ReadyTag,
                timeout: TimeSpan.FromSeconds(5))

            // Readiness: Keycloak OIDC discovery endpoint
            .AddCheck<KeycloakHealthCheck>(
                "keycloak_oidc",
                failureStatus: HealthStatus.Unhealthy,
                tags: ReadyTag)

            // Readiness: JWT signing keys available for token validation
            .AddCheck<JwtSigningKeysHealthCheck>(
                "jwt_signing_keys",
                failureStatus: HealthStatus.Unhealthy,
                tags: ReadyTag)

            // Readiness: GraphQL schema built successfully
            .AddCheck<GraphQLSchemaHealthCheck>(
                "graphql_schema",
                failureStatus: HealthStatus.Degraded,
                tags: ReadyTag);

        return services;
    }
}
