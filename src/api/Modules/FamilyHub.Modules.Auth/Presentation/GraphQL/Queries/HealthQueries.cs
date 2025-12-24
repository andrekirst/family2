namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for health and status checks.
/// </summary>
[ExtendObjectType("Query")]
public sealed class HealthQueries
{
    /// <summary>
    /// Returns server status and timestamp.
    /// </summary>
    /// <returns>Health status information.</returns>
    public HealthStatus GetHealth()
    {
        return new HealthStatus
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Service = "Auth Module"
        };
    }
}

/// <summary>
/// Represents the health status of the service.
/// </summary>
public sealed record HealthStatus
{
    public required string Status { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string Service { get; init; }
}
