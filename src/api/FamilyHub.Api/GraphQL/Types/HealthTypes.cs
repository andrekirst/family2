using HotChocolate.Types;

namespace FamilyHub.Api.GraphQL.Types;

/// <summary>
/// Basic liveness check response.
/// </summary>
public sealed record HealthLiveness
{
    /// <summary>
    /// Status of the service ("healthy", "degraded", "unhealthy").
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Current server time (UTC).
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Service version.
    /// </summary>
    public required string Version { get; init; }
}

/// <summary>
/// Detailed health check response with dependency status.
/// </summary>
public sealed record HealthDetails
{
    /// <summary>
    /// Overall status of the service.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Current server time (UTC).
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Service version.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Total uptime of the service.
    /// </summary>
    public required TimeSpan Uptime { get; init; }

    /// <summary>
    /// Status of each dependency.
    /// </summary>
    public required IReadOnlyList<DependencyHealth> Dependencies { get; init; }
}

/// <summary>
/// Health status of a single dependency (database, message queue, etc.).
/// </summary>
public sealed record DependencyHealth
{
    /// <summary>
    /// Name of the dependency (e.g., "PostgreSQL", "RabbitMQ", "Redis").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Status of the dependency ("healthy", "degraded", "unhealthy").
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Optional message providing more details about the status.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Response time for the health check (if applicable).
    /// </summary>
    public TimeSpan? ResponseTime { get; init; }
}

/// <summary>
/// GraphQL ObjectType for <see cref="HealthLiveness"/>.
/// </summary>
public sealed class HealthLivenessType : ObjectType<HealthLiveness>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<HealthLiveness> descriptor)
    {
        descriptor.Name("HealthLiveness");
        descriptor.Description("Basic liveness check response.");

        descriptor.Field(h => h.Status).Type<NonNullType<StringType>>();
        descriptor.Field(h => h.Timestamp).Type<NonNullType<DateTimeType>>();
        descriptor.Field(h => h.Version).Type<NonNullType<StringType>>();
    }
}

/// <summary>
/// GraphQL ObjectType for <see cref="HealthDetails"/>.
/// </summary>
public sealed class HealthDetailsType : ObjectType<HealthDetails>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<HealthDetails> descriptor)
    {
        descriptor.Name("HealthDetails");
        descriptor.Description("Detailed health check with dependency status.");

        descriptor.Field(h => h.Status).Type<NonNullType<StringType>>();
        descriptor.Field(h => h.Timestamp).Type<NonNullType<DateTimeType>>();
        descriptor.Field(h => h.Version).Type<NonNullType<StringType>>();
        descriptor.Field(h => h.Uptime).Type<NonNullType<TimeSpanType>>();
        descriptor.Field(h => h.Dependencies)
            .Type<NonNullType<ListType<NonNullType<DependencyHealthType>>>>();
    }
}

/// <summary>
/// GraphQL ObjectType for <see cref="DependencyHealth"/>.
/// </summary>
public sealed class DependencyHealthType : ObjectType<DependencyHealth>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<DependencyHealth> descriptor)
    {
        descriptor.Name("DependencyHealth");
        descriptor.Description("Health status of a dependency.");

        descriptor.Field(d => d.Name).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.Status).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.Message).Type<StringType>();
        descriptor.Field(d => d.ResponseTime).Type<TimeSpanType>();
    }
}
