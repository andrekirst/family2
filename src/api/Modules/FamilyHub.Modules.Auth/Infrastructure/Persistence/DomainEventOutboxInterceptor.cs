using System.Text.Json;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FamilyHub.Modules.Auth.Infrastructure.Persistence;

/// <summary>
/// EF Core interceptor that automatically saves domain events to the outbox table.
/// Implements the Transactional Outbox pattern to ensure domain events are not lost.
/// </summary>
/// <remarks>
/// <para><strong>Pattern:</strong></para>
/// <para>
/// Before SaveChanges commits, this interceptor:
/// 1. Collects all domain events from tracked aggregates
/// 2. Converts each event to an OutboxEvent entity (with JSON payload)
/// 3. Adds OutboxEvent entities to the same transaction
/// 4. Clears domain events from aggregates
/// 5. Lets SaveChanges proceed atomically
/// </para>
/// <para><strong>Why this works:</strong></para>
/// <para>
/// Domain changes and outbox entries are committed in a single database transaction.
/// If either fails, both roll back. Background worker later publishes events from outbox.
/// </para>
/// </remarks>
public sealed class DomainEventOutboxInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            SaveDomainEventsToOutbox(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            SaveDomainEventsToOutbox(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private static void SaveDomainEventsToOutbox(DbContext context)
    {
        // 1. Find all tracked aggregate roots with domain events
        var aggregatesWithEvents = context.ChangeTracker
            .Entries<AggregateRoot<object>>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        if (aggregatesWithEvents.Count == 0)
        {
            return;
        }

        // 2. Collect all domain events
        var domainEvents = aggregatesWithEvents
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToList();

        // 3. Convert each domain event to an OutboxEvent
        var outboxEvents = domainEvents.Select(ConvertToOutboxEvent).ToList();

        // 4. Add outbox events to the context (same transaction as domain changes)
        context.Set<OutboxEvent>().AddRange(outboxEvents);

        // 5. Clear domain events from aggregates (prevent duplicate processing)
        foreach (var aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }
    }

    private static OutboxEvent ConvertToOutboxEvent(DomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();
        var eventTypeName = eventType.FullName ?? eventType.Name;

        // Get aggregate type and ID from event properties
        var aggregateType = ExtractAggregateType(domainEvent);
        var aggregateId = ExtractAggregateId(domainEvent);

        // Get event version (all events have EventVersion property per design)
        var eventVersion = ExtractEventVersion(domainEvent);

        // Serialize event to JSON
        var payload = JsonSerializer.Serialize(domainEvent, eventType, JsonOptions);

        return OutboxEvent.Create(
            eventType: eventTypeName,
            eventVersion: eventVersion,
            aggregateType: aggregateType,
            aggregateId: aggregateId,
            payload: payload
        );
    }

    private static string ExtractAggregateType(DomainEvent domainEvent)
    {
        // Extract aggregate type from event type name
        // Example: "FamilyMemberInvitedEvent" -> "FamilyMemberInvitation"
        //          "InvitationAcceptedEvent" -> "FamilyMemberInvitation"
        //          "ManagedAccountCreatedEvent" -> "User"

        var eventTypeName = domainEvent.GetType().Name;

        // For FamilyMemberInvitedEvent, InvitationAcceptedEvent, InvitationCanceledEvent
        if (eventTypeName.Contains("Invitation") || eventTypeName.Contains("FamilyMember"))
        {
            return "FamilyMemberInvitation";
        }

        // For ManagedAccountCreatedEvent
        if (eventTypeName.Contains("ManagedAccount"))
        {
            return "User";
        }

        // Default: Remove "Event" suffix
        return eventTypeName.Replace("Event", string.Empty);
    }

    private static Guid ExtractAggregateId(DomainEvent domainEvent)
    {
        // All domain events have an aggregate ID property
        // Look for common patterns: InvitationId, UserId, FamilyId, etc.

        var eventType = domainEvent.GetType();

        // Try InvitationId first (most common in Auth module)
        var invitationIdProp = eventType.GetProperty("InvitationId");
        if (invitationIdProp != null)
        {
            var invitationId = invitationIdProp.GetValue(domainEvent);
            if (invitationId != null)
            {
                // Vogen value object - get .Value property
                var valueProp = invitationId.GetType().GetProperty("Value");
                if (valueProp != null)
                {
                    var guidValue = valueProp.GetValue(invitationId);
                    if (guidValue is Guid guid)
                    {
                        return guid;
                    }
                }
            }
        }

        // Try UserId
        var userIdProp = eventType.GetProperty("UserId");
        if (userIdProp != null)
        {
            var userId = userIdProp.GetValue(domainEvent);
            if (userId != null)
            {
                var valueProp = userId.GetType().GetProperty("Value");
                if (valueProp != null)
                {
                    var guidValue = valueProp.GetValue(userId);
                    if (guidValue is Guid guid)
                    {
                        return guid;
                    }
                }
            }
        }

        // Default: Use event's EventId
        return domainEvent.EventId;
    }

    private static int ExtractEventVersion(DomainEvent domainEvent)
    {
        // All domain events have EventVersion property per design
        var eventVersionProp = domainEvent.GetType().GetProperty("EventVersion");
        if (eventVersionProp != null)
        {
            var versionValue = eventVersionProp.GetValue(domainEvent);
            if (versionValue is int version)
            {
                return version;
            }
        }

        // Default to version 1 if not found
        return 1;
    }
}
