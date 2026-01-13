using FamilyHub.SharedKernel.Domain;
using OutboxEventId = FamilyHub.Modules.Auth.Domain.ValueObjects.OutboxEventId;

namespace FamilyHub.Modules.Auth.Domain;

/// <summary>
/// Represents a domain event stored in the outbox for reliable publishing.
/// Implements the Transactional Outbox pattern to ensure domain events are not lost.
/// </summary>
public class OutboxEvent : Entity<OutboxEventId>
{
    /// <summary>
    /// Fully qualified name of the event type (e.g., "FamilyHub.Modules.Auth.Domain.Events.FamilyMemberInvitedEvent").
    /// </summary>
    public string EventType { get; private set; } = null!;

    /// <summary>
    /// Event schema version for future evolution and migration.
    /// </summary>
    public int EventVersion { get; private set; }

    /// <summary>
    /// Type of aggregate that raised the event (e.g., "FamilyMemberInvitation", "User").
    /// </summary>
    public string AggregateType { get; private set; } = null!;

    /// <summary>
    /// Unique identifier of the aggregate that raised this event.
    /// </summary>
    public Guid AggregateId { get; private set; }

    /// <summary>
    /// Event data serialized as JSON.
    /// </summary>
    public string Payload { get; private set; } = null!;

    /// <summary>
    /// When the event was processed and published to RabbitMQ.
    /// Null if not yet processed.
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// Current processing status of the event.
    /// </summary>
    public OutboxEventStatus Status { get; private set; }

    /// <summary>
    /// Number of times publishing this event has been attempted.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Error message from the most recent failed publishing attempt.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    // Private constructor for EF Core
    private OutboxEvent() : base(OutboxEventId.From(Guid.Empty))
    {
    }

    private OutboxEvent(OutboxEventId id) : base(id)
    {
    }

    /// <summary>
    /// Creates a new outbox event from a domain event.
    /// </summary>
    public static OutboxEvent Create(
        string eventType,
        int eventVersion,
        string aggregateType,
        Guid aggregateId,
        string payload)
    {
        return new OutboxEvent(OutboxEventId.New())
        {
            EventType = eventType,
            EventVersion = eventVersion,
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            Payload = payload,
            Status = OutboxEventStatus.PENDING,
            RetryCount = 0
        };
    }

    /// <summary>
    /// Marks the event as successfully processed.
    /// </summary>
    public void MarkAsProcessed()
    {
        Status = OutboxEventStatus.PROCESSED;
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    /// <summary>
    /// Marks the event as failed with an error message.
    /// Increments the retry count but keeps status as Pending for automatic retry.
    /// </summary>
    /// <remarks>
    /// Events remain in Pending status even after failures to enable infinite retry.
    /// Manual intervention required to mark as Failed permanently.
    /// </remarks>
    public void MarkAsFailedWithRetry(string errorMessage)
    {
        // Keep status as Pending for automatic retry
        ErrorMessage = errorMessage;
        RetryCount++;
    }

    /// <summary>
    /// Marks the event as permanently failed (requires manual intervention).
    /// </summary>
    public void MarkAsPermanentlyFailed(string errorMessage)
    {
        Status = OutboxEventStatus.FAILED;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Resets the event to pending status for retry.
    /// </summary>
    public void ResetToPending()
    {
        Status = OutboxEventStatus.PENDING;
        ProcessedAt = null;
    }

    /// <summary>
    /// Increments the retry count without changing status.
    /// Used when retrying within the same processing cycle.
    /// </summary>
    public void IncrementRetryCount() => RetryCount++;
}

/// <summary>
/// Status of an outbox event.
/// </summary>
public enum OutboxEventStatus
{
    /// <summary>
    /// Event is waiting to be published.
    /// </summary>
    PENDING = 0,

    /// <summary>
    /// Event has been successfully published to RabbitMQ.
    /// </summary>
    PROCESSED = 1,

    /// <summary>
    /// Event publishing failed after maximum retry attempts.
    /// Requires manual intervention.
    /// </summary>
    FAILED = 2
}
