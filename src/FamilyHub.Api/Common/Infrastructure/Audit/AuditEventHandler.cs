using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Common.Infrastructure.Audit;

/// <summary>
/// Domain event observer that persists every domain event as an audit record.
/// Registered as IDomainEventObserver and invoked by DomainEventPublishingBehavior
/// after each event is published, ensuring full audit trail coverage.
///
/// Errors are caught and logged to prevent audit failures from breaking command processing.
/// </summary>
public sealed class AuditEventHandler(
    IAuditEventPersister persister,
    ILogger<AuditEventHandler> logger) : IDomainEventObserver
{
    public async Task OnEventPublishedAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            await persister.PersistAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log but don't throw — audit persistence should never fail the command
            logger.LogError(ex,
                "Failed to persist audit event {EventType} ({EventId})",
                @event.GetType().Name,
                @event.EventId);
        }
    }
}
