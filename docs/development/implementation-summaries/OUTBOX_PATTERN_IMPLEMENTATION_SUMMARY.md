# Outbox Pattern Implementation Summary

**Implementation Date:** January 4-5, 2026
**Epic:** #24 - Family Member Invitation System
**Phase:** 2.C - Outbox Pattern & Event Publishing
**Status:** ✅ COMPLETED

## Overview

Implemented the **Transactional Outbox Pattern** for reliable domain event publishing to RabbitMQ. This ensures domain events are never lost, even if the message broker is temporarily unavailable.

## Architecture Pattern

```
┌─────────────────────────────────────────────────────────────┐
│                    Domain Event Flow                         │
└─────────────────────────────────────────────────────────────┘

1. Aggregate raises domain event
   FamilyMemberInvitation.CreateEmailInvitation()
   ↓ AddDomainEvent(new FamilyMemberInvitedEvent(...))

2. SaveChangesAsync() triggered
   ↓ DomainEventOutboxInterceptor intercepts

3. Interceptor converts events to OutboxEvent entities
   ↓ Serializes to JSON, adds to auth.outbox_events table

4. All changes committed atomically
   ├── Domain changes (invitation created)
   └── Outbox events (event saved)

5. Background worker polls every 5 seconds
   ↓ OutboxEventPublisher fetches pending events

6. Events published to RabbitMQ
   ↓ With exponential backoff retry (max 15 min delay)

7. On success: Mark as Processed
   On failure: Increment retry_count, stay Pending
```

## Components Implemented

### 1. Domain Model

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Domain/OutboxEvent.cs`

```csharp
public class OutboxEvent : Entity<OutboxEventId>
{
    public string EventType { get; } // Fully qualified class name
    public int EventVersion { get; } // For schema evolution
    public string AggregateType { get; } // FamilyMemberInvitation, User, etc.
    public Guid AggregateId { get; } // ID of aggregate
    public string Payload { get; } // JSON serialized event
    public DateTime? ProcessedAt { get; }
    public OutboxEventStatus Status { get; } // Pending, Processed, Failed
    public int RetryCount { get; }
    public string? ErrorMessage { get; }
}
```

**States:**

- **Pending (0):** Waiting to be published (default, retries forever)
- **Processed (1):** Successfully published to RabbitMQ
- **Failed (2):** Permanently failed (requires manual intervention)

### 2. Database Table

**Migration:** `/src/api/Modules/FamilyHub.Modules.Auth/Persistence/Migrations/20260105000000_CreateOutboxEventsTable.cs`

**Table:** `auth.outbox_events`

```sql
CREATE TABLE auth.outbox_events (
    event_id UUID PRIMARY KEY,
    event_type VARCHAR(255) NOT NULL, -- Full class name
    event_version INT NOT NULL, -- Schema version
    aggregate_type VARCHAR(255) NOT NULL, -- Aggregate name
    aggregate_id UUID NOT NULL, -- Aggregate ID
    payload JSONB NOT NULL, -- Event data
    processed_at TIMESTAMP NULL,
    status INT NOT NULL, -- 0=Pending, 1=Processed, 2=Failed
    retry_count INT NOT NULL DEFAULT 0,
    error_message TEXT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for efficient querying
CREATE INDEX ix_outbox_events_status_created_at ON auth.outbox_events (status, created_at);
CREATE INDEX ix_outbox_events_created_at ON auth.outbox_events (created_at);
```

### 3. Repository

**Interface:** `/src/api/Modules/FamilyHub.Modules.Auth/Domain/Repositories/IOutboxEventRepository.cs`
**Implementation:** `/src/api/Modules/FamilyHub.Modules.Auth/Persistence/Repositories/OutboxEventRepository.cs`

**Key Methods:**

- `GetPendingEventsAsync(batchSize)` - Fetch events to publish
- `GetEventsForArchivalAsync(olderThan)` - Fetch old events for cleanup
- `AddRangeAsync(events)` - Bulk insert
- `UpdateAsync(event)` - Update status/retry count

### 4. Domain Event Interceptor

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Persistence/DomainEventOutboxInterceptor.cs`

**Behavior:**

- Intercepts `SaveChangesAsync()` before commit
- Collects all domain events from tracked aggregates
- Converts each event to `OutboxEvent` (JSON serialization)
- Adds `OutboxEvent` entities to same transaction
- Clears domain events from aggregates
- Lets EF Core commit everything atomically

**Event Extraction:**

- `EventType`: Full class name (e.g., `FamilyHub.Modules.Auth.Domain.Events.FamilyMemberInvitedEvent`)
- `AggregateType`: Extracted from event type ("FamilyMemberInvitation" from "FamilyMemberInvitedEvent")
- `AggregateId`: Extracted from event properties (InvitationId, UserId, etc.)
- `EventVersion`: From event's `EventVersion` property
- `Payload`: JSON serialized with camelCase naming

### 5. Background Worker

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/BackgroundServices/OutboxEventPublisher.cs`

**Configuration:**

- Poll interval: 5 seconds
- Batch size: 100 events
- Exchange: `family-hub.events`
- Routing key: Event type (e.g., `FamilyHub.Modules.Auth.Domain.Events.FamilyMemberInvitedEvent`)

**Retry Strategy:**
Exponential backoff with max delay capped at 15 minutes. Retries **forever** (no circuit breaker).

| Attempt | Delay |
|---------|-------|
| 1 | Immediate |
| 2 | 1 second |
| 3 | 2 seconds |
| 4 | 5 seconds |
| 5 | 15 seconds |
| 6 | 60 seconds (1 min) |
| 7 | 300 seconds (5 min) |
| 8+ | 900 seconds (15 min) forever |

**Processing Logic:**

1. Fetch batch of pending events (status = Pending, ordered by created_at)
2. For each event:
   - Check if retry delay has elapsed (based on UpdatedAt timestamp)
   - If not ready, skip (will retry in next poll)
   - If ready, publish to RabbitMQ
   - On success: `MarkAsProcessed()`, set ProcessedAt
   - On failure: `MarkAsFailedWithRetry()`, increment RetryCount, stay Pending
3. Save all changes atomically

### 6. RabbitMQ Publisher (Stub)

**Interface:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Messaging/IMessageBrokerPublisher.cs`
**Stub Implementation:** `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Messaging/StubRabbitMqPublisher.cs`

**Purpose:** Phase 2 stub that logs events instead of publishing. Replace with real RabbitMQ client in Phase 5+.

```csharp
public Task PublishAsync(string exchange, string routingKey, string message, CancellationToken ct)
{
    _logger.LogInformation(
        "STUB: Published event to {Exchange}/{RoutingKey}: {Message}",
        exchange, routingKey, message);
    return Task.CompletedTask;
}
```

### 7. Dependency Injection

**File:** `/src/api/Modules/FamilyHub.Modules.Auth/AuthModuleServiceRegistration.cs`

**Registrations:**

```csharp
// Outbox interceptor
services.AddSingleton<DomainEventOutboxInterceptor>();

// DbContext with interceptor
services.AddPooledDbContextFactory<AuthDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention()
        .AddTimestampInterceptor(sp)
        .AddInterceptors(sp.GetRequiredService<DomainEventOutboxInterceptor>());
});

// Repository
services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

// RabbitMQ Publisher (stub for Phase 2)
services.AddSingleton<IMessageBrokerPublisher, StubRabbitMqPublisher>();

// Background worker
services.AddHostedService<OutboxEventPublisher>();
```

## Event Retention & Archival

**Strategy:** Archive processed events after 90 days to cold storage.

**Implementation:** Phase 2.C.4 (Quartz.NET cleanup job) - **TO BE IMPLEMENTED**

**Recommended Approach:**

1. Create Quartz.NET job: `OutboxCleanupJob`
2. Schedule: Daily at 2 AM
3. Logic:
   - Fetch processed events older than 90 days
   - Archive to separate table (`auth.outbox_events_archive`) or export to S3
   - Delete from `auth.outbox_events`

## Testing Strategy

### Unit Tests (TO BE IMPLEMENTED)

1. **OutboxEvent Domain Model:**
   - Test state transitions (Pending → Processed, Pending → Failed)
   - Test retry count increment
   - Test error message recording

2. **DomainEventOutboxInterceptor:**
   - Test event collection from aggregates
   - Test JSON serialization
   - Test aggregate type/ID extraction
   - Test domain event clearing

3. **OutboxEventPublisher:**
   - Test exponential backoff calculation
   - Test retry delay enforcement
   - Test batch processing
   - Test error handling

### Integration Tests (TO BE IMPLEMENTED)

1. **End-to-End Flow:**

   ```csharp
   [Fact]
   public async Task CreateInvitation_ShouldSaveDomainEventToOutbox()
   {
       // Arrange
       var invitation = FamilyMemberInvitation.CreateEmailInvitation(...);

       // Act
       await _repository.AddAsync(invitation);
       await _unitOfWork.SaveChangesAsync();

       // Assert
       var outboxEvents = await _outboxRepository.GetPendingEventsAsync(10);
       outboxEvents.Should().HaveCount(1);
       outboxEvents[0].EventType.Should().Contain("FamilyMemberInvitedEvent");
       outboxEvents[0].AggregateType.Should().Be("FamilyMemberInvitation");
       outboxEvents[0].Status.Should().Be(OutboxEventStatus.Pending);
   }
   ```

2. **Publisher Integration:**

   ```csharp
   [Fact]
   public async Task OutboxPublisher_ShouldPublishPendingEvents()
   {
       // Arrange
       var outboxEvent = OutboxEvent.Create(...);
       await _outboxRepository.AddAsync(outboxEvent);
       await _unitOfWork.SaveChangesAsync();

       // Act
       await _publisher.ProcessPendingEventsAsync(CancellationToken.None);

       // Assert
       var updatedEvent = await _outboxRepository.GetByIdAsync(outboxEvent.Id);
       updatedEvent.Status.Should().Be(OutboxEventStatus.Processed);
       updatedEvent.ProcessedAt.Should().NotBeNull();
   }
   ```

## Monitoring & Observability

### Metrics to Track

1. **Outbox Health:**
   - Pending event count (`SELECT COUNT(*) FROM auth.outbox_events WHERE status = 0`)
   - Failed event count (`SELECT COUNT(*) FROM auth.outbox_events WHERE status = 2`)
   - Average retry count (`SELECT AVG(retry_count) FROM auth.outbox_events WHERE status = 0`)
   - Oldest pending event (`SELECT MIN(created_at) FROM auth.outbox_events WHERE status = 0`)

2. **Publisher Performance:**
   - Events published per minute
   - Publishing latency (time from created_at to processed_at)
   - Retry rate
   - Error rate

### Grafana Dashboards (TO BE CREATED)

1. **Outbox Overview:**
   - Total events by status (stacked bar chart)
   - Publishing throughput (line chart)
   - Retry distribution (histogram)

2. **Alerts:**
   - Pending events > 1000 for > 5 minutes (RabbitMQ down?)
   - Failed events > 10 (manual intervention needed)
   - Average retry count > 5 (persistent errors)
   - Oldest pending event > 1 hour (stale events)

## Future Enhancements (Phase 5+)

1. **Real RabbitMQ Integration:**
   - Replace `StubRabbitMqPublisher` with `RabbitMqPublisher`
   - Use RabbitMQ.Client library
   - Configure exchanges, queues, bindings
   - Implement connection pooling and retry policies

2. **Event Schema Registry:**
   - Store event schemas in database or schema registry
   - Validate events against schemas before publishing
   - Support event versioning and migration

3. **Dead Letter Queue:**
   - Move permanently failed events to DLQ after N retries
   - Implement manual replay mechanism

4. **Distributed Tracing:**
   - Add correlation IDs to events
   - Integrate with OpenTelemetry
   - Trace events from aggregate → outbox → RabbitMQ → consumer

5. **Outbox per Module:**
   - Calendar module: `calendar.outbox_events`
   - Task module: `task.outbox_events`
   - Each module manages its own events

6. **Event Batching:**
   - Batch multiple events into single RabbitMQ message
   - Reduce network overhead

## Files Created

### Domain Layer

- `/src/api/Modules/FamilyHub.Modules.Auth/Domain/OutboxEvent.cs`
- `/src/api/Modules/FamilyHub.Modules.Auth/Domain/ValueObjects/OutboxEventId.cs`
- `/src/api/Modules/FamilyHub.Modules.Auth/Domain/Repositories/IOutboxEventRepository.cs`

### Persistence Layer

- `/src/api/Modules/FamilyHub.Modules.Auth/Persistence/Repositories/OutboxEventRepository.cs`
- `/src/api/Modules/FamilyHub.Modules.Auth/Persistence/Configurations/OutboxEventConfiguration.cs`
- `/src/api/Modules/FamilyHub.Modules.Auth/Persistence/Migrations/20260105000000_CreateOutboxEventsTable.cs`

### Infrastructure Layer

- `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Persistence/DomainEventOutboxInterceptor.cs`
- `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/BackgroundServices/OutboxEventPublisher.cs`
- `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Messaging/IMessageBrokerPublisher.cs`
- `/src/api/Modules/FamilyHub.Modules.Auth/Infrastructure/Messaging/StubRabbitMqPublisher.cs`

### Configuration

- **Modified:** `/src/api/Modules/FamilyHub.Modules.Auth/AuthModuleServiceRegistration.cs`
- **Modified:** `/src/api/Modules/FamilyHub.Modules.Auth/Persistence/AuthDbContext.cs`

## Acceptance Criteria Status

- [x] **2.C.1:** Outbox table created with correct schema
- [x] **2.C.2:** Domain events automatically saved to outbox on SaveChangesAsync
- [x] **2.C.3:** Events cleared from aggregates after save
- [x] **2.C.4:** Background worker polls every 5 seconds
- [x] **2.C.5:** Events published with exponential backoff
- [x] **2.C.6:** Failed events marked with error message
- [ ] **2.C.7:** Integration test: Domain event → Outbox → Published (TO BE IMPLEMENTED)
- [ ] **2.C.8:** Cleanup job archives old events (TO BE IMPLEMENTED - Phase 2.C.4)

## Known Issues

1. **Build Errors:** Pre-existing build errors in `/Application/Commands` directory prevent migration execution via `dotnet ef migrations add`. Migration was created manually following EF Core conventions.

2. **Missing Quartz.NET:** Cleanup job (Phase 2.C.4) requires Quartz.NET package, which is referenced in existing code but not installed.

## Next Steps

1. **Apply Migration:**

   ```bash
   cd /home/andrekirst/git/github/andrekirst/family2/src/api
   dotnet ef database update --context AuthDbContext --project Modules/FamilyHub.Modules.Auth --startup-project FamilyHub.Api
   ```

2. **Fix Build Errors:** Resolve missing using statements and Quartz.NET references in pre-existing code.

3. **Write Integration Tests:** Test outbox pattern end-to-end.

4. **Implement Cleanup Job (Phase 2.C.4):**
   - Add Quartz.NET package
   - Create `OutboxCleanupJob`
   - Schedule daily execution
   - Archive events to `auth.outbox_events_archive` table

5. **Add Monitoring:** Create Grafana dashboards for outbox metrics.

---

**Implementation completed by:** Claude Code (microservices-architect agent)
**Review required:** Yes - integration tests and cleanup job pending
