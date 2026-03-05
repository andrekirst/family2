using HotChocolate.Execution;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FamilyHub.Api.Common.Infrastructure.HealthChecks;

/// <summary>
/// Verifies the Hot Chocolate GraphQL schema can be built and contains expected types.
/// This catches type extension conflicts, missing DI registrations, and other schema build failures
/// that would otherwise silently cause /graphql to return 404.
/// </summary>
public class GraphQLSchemaHealthCheck(IRequestExecutorResolver executorResolver) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var executor = await executorResolver.GetRequestExecutorAsync(cancellationToken: cancellationToken);
            var schema = executor.Schema;
            var typeCount = schema.Types.Count;

            return typeCount > 0
                ? HealthCheckResult.Healthy($"{typeCount} types loaded")
                : HealthCheckResult.Unhealthy("GraphQL schema contains no types");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("GraphQL schema failed to build", ex);
        }
    }
}
