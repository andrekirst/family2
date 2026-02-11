namespace FamilyHub.Common.Domain;

/// <summary>
/// Base record for domain events with automatic EventId and timestamp.
/// Implements both our IDomainEvent and Mediator's INotification for
/// dual-purpose: domain event collection and Mediator publishing.
/// </summary>
public abstract record DomainEvent : IDomainEvent, Mediator.INotification
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
