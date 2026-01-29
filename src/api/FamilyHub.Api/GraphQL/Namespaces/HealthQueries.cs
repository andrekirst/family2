namespace FamilyHub.Api.GraphQL.Namespaces;

/// <summary>
/// Namespace container for health check queries.
/// Accessed via query { health { ... } }.
/// </summary>
/// <remarks>
/// <para>
/// This namespace provides health check endpoints:
/// <list type="bullet">
/// <item><description>liveness - Basic up/down status (no auth required)</description></item>
/// <item><description>details - Detailed health with dependencies (requires auth)</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record HealthQueries;
