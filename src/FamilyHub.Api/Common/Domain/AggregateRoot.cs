namespace FamilyHub.Api.Common.Domain;

/// <summary>
/// Base class for aggregate roots in the domain model.
/// Aggregates encapsulate business logic and enforce invariants.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identifier (value object)</typeparam>
public abstract class AggregateRoot<TId> where TId : struct
{
    public TId Id { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this aggregate during its lifecycle.
    /// Events are published after SaveChanges succeeds.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raise a domain event that will be published after the aggregate is persisted.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clear all domain events. Called by DbContext after publishing events.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
