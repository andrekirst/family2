namespace FamilyHub.Api.Common.Domain;

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
}
