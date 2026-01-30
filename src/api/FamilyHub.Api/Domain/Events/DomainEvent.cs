using MediatR;

namespace FamilyHub.Api.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// Uses MediatR's INotification for in-process event dispatching.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
}

/// <summary>
/// Base class for domain events with automatic timestamp.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
