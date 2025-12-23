using MediatR;

namespace FamilyHub.SharedKernel.Domain;

/// <summary>
/// Base class for all domain events.
/// Domain events represent something important that happened in the domain.
/// </summary>
public abstract class DomainEvent : INotification
{
    /// <summary>
    /// Gets the unique identifier for this event.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when this event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
