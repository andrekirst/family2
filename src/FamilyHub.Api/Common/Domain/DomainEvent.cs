namespace FamilyHub.Api.Common.Domain;

/// <summary>
/// Base record for domain events with automatic EventId and timestamp.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}