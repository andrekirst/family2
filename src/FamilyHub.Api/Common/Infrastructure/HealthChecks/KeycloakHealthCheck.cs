using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Common.Infrastructure.HealthChecks;

/// <summary>
/// Verifies Keycloak OIDC discovery endpoint is reachable and returns valid configuration.
/// </summary>
public class KeycloakHealthCheck(IOptionsMonitor<JwtBearerOptions> jwtOptions) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme);
            if (options.ConfigurationManager is null)
                return HealthCheckResult.Degraded("OIDC ConfigurationManager not configured");

            var config = await options.ConfigurationManager.GetConfigurationAsync(cancellationToken);
            if (string.IsNullOrEmpty(config.Issuer))
                return HealthCheckResult.Unhealthy("OIDC discovery returned empty issuer");

            return HealthCheckResult.Healthy($"OIDC discovery loaded (issuer: {config.Issuer})");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Keycloak OIDC discovery failed", ex);
        }
    }
}
