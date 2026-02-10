using FamilyHub.Api.Common.Application;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Outermost pipeline behavior that publishes domain events after the inner
/// pipeline (including transaction commit) completes. Events were collected
/// by the DomainEventInterceptor during SaveChanges.
///
/// Publishing is wrapped in try-catch so that failures (e.g. email send)
/// don't surface as command failures.
/// </summary>
public sealed class DomainEventPublishingBehavior<TMessage, TResponse>(
    IDomainEventCollector collector,
    IMediator mediator,
    ILogger<DomainEventPublishingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);

        var events = collector.GetAndClearEvents();

        foreach (var domainEvent in events)
        {
            try
            {
                if (domainEvent is INotification notification)
                {
                    await mediator.Publish(notification, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error publishing domain event {EventType} ({EventId})",
                    domainEvent.GetType().Name,
                    domainEvent.EventId);
            }
        }

        return response;
    }
}
