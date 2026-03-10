using System.Diagnostics;
using FamilyHub.Api.Common.Modules;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using Mediator;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Outermost pipeline behavior that publishes domain events after the inner
/// pipeline (including transaction commit) completes. Events were collected
/// by the DomainEventInterceptor during SaveChanges.
///
/// Enriches each event with metadata before publishing:
/// - ActorUserId from the current authenticated user (via IRequireUser on the message)
/// - CorrelationId from Activity.Current.TraceId (for distributed tracing)
/// - CausationId linking child events to their parent event
///
/// Publishing is wrapped in try-catch so that failures (e.g. email send)
/// don't surface as command failures.
/// </summary>
[PipelinePriority(PipelinePriorities.DomainEventPublishing)]
public sealed class DomainEventPublishingBehavior<TMessage, TResponse>(
    IDomainEventCollector collector,
    IMediator mediator,
    IEnumerable<IDomainEventObserver> observers,
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

        // Resolve actor context once for all events in this batch
        var actorUserId = ResolveActorUserId(message);
        var correlationId = Activity.Current?.TraceId.ToString();

        foreach (var domainEvent in events)
        {
            // Enrich event with metadata if it's a DomainEvent record (supports 'with')
            var enrichedEvent = EnrichEvent(domainEvent, actorUserId, correlationId);

            // Publish via Mediator for specific INotificationHandler<T> subscribers
            try
            {
                if (enrichedEvent is INotification notification)
                {
                    await mediator.Publish(notification, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error publishing domain event {EventType} ({EventId})",
                    enrichedEvent.GetType().Name,
                    enrichedEvent.EventId);
            }

            // Notify all domain event observers (e.g. chain trigger handler)
            foreach (var observer in observers)
            {
                try
                {
                    await observer.OnEventPublishedAsync(enrichedEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Error in domain event observer {ObserverType} for {EventType} ({EventId})",
                        observer.GetType().Name,
                        enrichedEvent.GetType().Name,
                        enrichedEvent.EventId);
                }
            }
        }

        return response;
    }

    private static Guid? ResolveActorUserId(TMessage message)
    {
        // Extract UserId from the message if UserResolutionBehavior already populated it
        // (avoids a redundant DB lookup since the inner pipeline already resolved the user)
        if (message is IRequireUser requireUser)
        {
            var userIdValue = requireUser.UserId.Value;
            return userIdValue != Guid.Empty ? userIdValue : null;
        }

        return null;
    }

    private static IDomainEvent EnrichEvent(IDomainEvent domainEvent, Guid? actorUserId, string? correlationId)
    {
        if (domainEvent is DomainEvent de)
        {
            return de with
            {
                ActorUserId = de.ActorUserId ?? actorUserId,
                CorrelationId = de.CorrelationId ?? correlationId,
            };
        }

        return domainEvent;
    }
}
