using System.Text.Json;
using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Common.Infrastructure.Audit;

/// <summary>
/// Persists domain events as immutable audit records in the audit_events table.
/// Serializes the full event payload as JSON for complete traceability.
/// </summary>
public sealed class AuditEventPersister(
    AppDbContext dbContext,
    ILogger<AuditEventPersister> logger) : IAuditEventPersister
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public async Task PersistAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType().Name;

        string? entityType = null;
        string? entityId = null;

        // Extract entity info if the event implements the marker interface
        if (domainEvent is IEntityDomainEvent entityEvent)
        {
            entityType = entityEvent.EntityType;
            entityId = entityEvent.EntityId;
        }

        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions);

        var auditEvent = new AuditEvent
        {
            EventType = eventType,
            EventId = domainEvent.EventId,
            OccurredAt = domainEvent.OccurredAt,
            ActorUserId = domainEvent.ActorUserId,
            CorrelationId = domainEvent.CorrelationId,
            CausationId = domainEvent.CausationId,
            EntityType = entityType,
            EntityId = entityId,
            Payload = payload,
        };

        dbContext.AuditEvents.Add(auditEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogDebug(
            "Persisted audit event {EventType} ({EventId}) for entity {EntityType}:{EntityId}",
            eventType,
            domainEvent.EventId,
            entityType ?? "(none)",
            entityId ?? "(none)");
    }
}
