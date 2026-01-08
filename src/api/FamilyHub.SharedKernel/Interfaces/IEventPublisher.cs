namespace FamilyHub.SharedKernel.Interfaces;

/// <summary>
/// Interface for publishing domain events.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a domain event to all registered handlers.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;
}
