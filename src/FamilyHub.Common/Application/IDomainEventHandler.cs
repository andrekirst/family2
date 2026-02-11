namespace FamilyHub.Common.Application;

/// <summary>
/// Interface for domain event handlers. Extends Mediator's INotificationHandler
/// for source-generated discovery while keeping our own abstraction layer.
/// </summary>
public interface IDomainEventHandler<in TEvent>
    : Mediator.INotificationHandler<TEvent>
    where TEvent : Mediator.INotification
{
    // Inherited: ValueTask Handle(TEvent notification, CancellationToken ct);
}
