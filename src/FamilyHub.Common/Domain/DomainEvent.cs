using System.Diagnostics;

namespace FamilyHub.Common.Domain;

/// <summary>
/// Base record for domain events with automatic EventId, timestamp, and metadata.
/// Implements both our IDomainEvent and Mediator's INotification for
/// dual-purpose: domain event collection and Mediator publishing.
///
/// Metadata properties (ActorUserId, CorrelationId, CausationId) are populated
/// by the DomainEventPublishingBehavior after events are collected from aggregates.
/// CorrelationId defaults to the current Activity's TraceId for distributed tracing.
/// </summary>
public abstract record DomainEvent : IDomainEvent, Mediator.INotification
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid? ActorUserId { get; init; }
    public string? CorrelationId { get; init; } = Activity.Current?.TraceId.ToString();
    public Guid? CausationId { get; init; }
}
