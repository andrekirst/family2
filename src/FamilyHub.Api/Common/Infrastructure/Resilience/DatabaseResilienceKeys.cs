namespace FamilyHub.Api.Common.Infrastructure.Resilience;

/// <summary>
/// Constants for named resilience pipeline keys used throughout the application.
/// </summary>
public static class ResiliencePipelineKeys
{
    /// <summary>
    /// Pipeline for transient PostgreSQL database errors (NpgsqlException with IsTransient, TimeoutException).
    /// </summary>
    public const string Database = "database";

    /// <summary>
    /// Pipeline for external HTTP client calls (retry + circuit breaker + timeout).
    /// </summary>
    public const string HttpClient = "http-client";
}
