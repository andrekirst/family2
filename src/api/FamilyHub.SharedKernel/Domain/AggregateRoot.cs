namespace FamilyHub.SharedKernel.Domain;

/// <summary>
/// Base class for all aggregate roots in the domain.
/// Inherits timestamp tracking from Entity base class.
/// </summary>
/// <remarks>
/// All aggregate roots automatically get CreatedAt/UpdatedAt timestamps because
/// Entity implements ITimestampable. This provides consistent audit trails across
/// all major domain entities without needing a separate AuditableAggregateRoot class.
/// </remarks>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the domain events that have been raised by this aggregate root.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the aggregate root with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for this aggregate root.</param>
    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Parameterless constructor required for EF Core entity materialization.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Adds a domain event to be published after the aggregate is persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events. Called after events have been published.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
