namespace FamilyHub.Api.Common.Infrastructure.Audit;

/// <summary>
/// Entity representing a persisted audit record of a domain event.
/// Maps to the public.audit_events table.
/// Immutable once written — provides full traceability of who did what, when, and why.
/// </summary>
public sealed class AuditEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Fully-qualified type name of the domain event (e.g. "UserRegisteredEvent").
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    /// The unique EventId from the domain event.
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// When the domain event originally occurred (UTC).
    /// </summary>
    public required DateTime OccurredAt { get; init; }

    /// <summary>
    /// The authenticated user who triggered the action, if known.
    /// </summary>
    public Guid? ActorUserId { get; init; }

    /// <summary>
    /// Distributed tracing correlation ID linking to the originating request.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// The EventId of the parent event that caused this event (event lineage).
    /// </summary>
    public Guid? CausationId { get; init; }

    /// <summary>
    /// The type of entity this event relates to (e.g. "Family", "User"), if available.
    /// </summary>
    public string? EntityType { get; init; }

    /// <summary>
    /// The ID of the entity this event relates to, if available.
    /// </summary>
    public string? EntityId { get; init; }

    /// <summary>
    /// Full JSON-serialized payload of the domain event.
    /// </summary>
    public required string Payload { get; init; }

    /// <summary>
    /// When this audit record was created (server-side default in DB).
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
