namespace FamilyHub.Common.Domain;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something important that happened in the domain.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred (UTC).
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// The user who triggered the action that raised this event.
    /// Populated by the pipeline behavior after the event is collected.
    /// </summary>
    Guid? ActorUserId { get; }

    /// <summary>
    /// Trace correlation ID linking this event to the originating request.
    /// Populated from Activity.Current.TraceId for distributed tracing integration.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// The EventId of the parent event that caused this event to be raised.
    /// Establishes event lineage when one event handler raises another event.
    /// </summary>
    Guid? CausationId { get; }
}
