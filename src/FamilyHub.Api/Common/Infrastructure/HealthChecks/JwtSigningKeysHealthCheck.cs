using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Common.Infrastructure.HealthChecks;

/// <summary>
/// Verifies JWT signing keys are available from Keycloak's OIDC discovery.
/// Without signing keys, no JWT token can be validated.
/// </summary>
public class JwtSigningKeysHealthCheck(IOptionsMonitor<JwtBearerOptions> jwtOptions) : IHealthCheck
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
            var keyCount = config.SigningKeys.Count;

            return keyCount > 0
                ? HealthCheckResult.Healthy($"{keyCount} signing key(s) available")
                : HealthCheckResult.Unhealthy("No signing keys found in OIDC discovery");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to retrieve JWT signing keys", ex);
        }
    }
}
