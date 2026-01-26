namespace FamilyHub.Modules.UserProfile.Persistence.Entities;

/// <summary>
/// EF Core entity for persisting profile events.
/// Maps domain ProfileEvent records to the database.
/// </summary>
/// <remarks>
/// This entity uses primitive types for database storage.
/// The ProfileEventStore handles mapping between domain events and this entity,
/// including JSON serialization of event data.
/// </remarks>
public class ProfileEventEntity
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The profile this event belongs to.
    /// </summary>
    public Guid ProfileId { get; set; }

    /// <summary>
    /// The type of event (e.g., "ProfileCreatedEvent", "ProfileFieldUpdatedEvent").
    /// Used for deserialization.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// JSON-serialized event data.
    /// Stored as JSONB in PostgreSQL for efficient querying.
    /// </summary>
    public string EventData { get; set; } = string.Empty;

    /// <summary>
    /// The user who initiated this change.
    /// </summary>
    public Guid ChangedBy { get; set; }

    /// <summary>
    /// When this event occurred.
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Sequential version number for ordering events.
    /// Unique per profile for optimistic concurrency.
    /// </summary>
    public int Version { get; set; }
}
