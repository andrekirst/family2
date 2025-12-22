using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.SharedKernel.Interfaces;

/// <summary>
/// Interface for handling domain events.
/// </summary>
public interface IEventHandler<in TEvent>
    where TEvent : DomainEvent
{
    /// <summary>
    /// Handles the specified domain event.
    /// </summary>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
