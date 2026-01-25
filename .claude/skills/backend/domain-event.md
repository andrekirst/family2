---
name: domain-event
description: Define and publish domain events with MediatR
category: backend
module-aware: true
inputs:
  - eventName: PascalCase event name (e.g., FamilyCreatedEvent)
  - module: DDD module name
  - fields: Event payload fields
---

# Domain Event Skill

Create a domain event with MediatR notification and optional RabbitMQ publishing.

## Context

Load module profile: `agent-os/profiles/modules/{module}.yaml`

## Steps

### 1. Define Event

**Location:** `Modules/FamilyHub.Modules.{Module}/Domain/Events/{EventName}.cs`

```csharp
using MediatR;
using FamilyHub.Modules.{Module}.Domain.ValueObjects;

namespace FamilyHub.Modules.{Module}.Domain.Events;

public sealed record {EventName}(
    {EntityId} Id,
    // Add event-specific fields
    DateTime CreatedAt
) : INotification;
```

### 2. Raise Event in Aggregate

```csharp
public sealed class {Entity} : AggregateRoot<{EntityId}>
{
    public static {Entity} Create({Parameters})
    {
        var entity = new {Entity}
        {
            Id = {EntityId}.New(),
            // Set properties
            CreatedAt = DateTime.UtcNow
        };

        entity.RaiseDomainEvent(new {EventName}(
            entity.Id,
            // Map fields
            entity.CreatedAt
        ));

        return entity;
    }
}
```

### 3. Create Event Handler

**Location:** `Modules/FamilyHub.Modules.{Module}/Application/EventHandlers/{EventName}Handler.cs`

```csharp
using MediatR;
using FamilyHub.Modules.{Module}.Domain.Events;

namespace FamilyHub.Modules.{Module}.Application.EventHandlers;

public sealed class {EventName}Handler
    : INotificationHandler<{EventName}>
{
    private readonly ILogger<{EventName}Handler> _logger;

    public {EventName}Handler(ILogger<{EventName}Handler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(
        {EventName} notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "{EventName} received for {Id}",
            nameof({EventName}),
            notification.Id);

        // Handle event (e.g., publish to RabbitMQ, update projections)
        await Task.CompletedTask;
    }
}
```

### 4. Optional: Publish to RabbitMQ

```csharp
public sealed class {EventName}Handler
    : INotificationHandler<{EventName}>
{
    private readonly IMessageBrokerPublisher _publisher;

    public {EventName}Handler(IMessageBrokerPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task Handle(
        {EventName} notification,
        CancellationToken cancellationToken)
    {
        // Publish for cross-module event chains
        await _publisher.PublishAsync(notification, cancellationToken);
    }
}
```

## Common Event Patterns

**Created Event:**

```csharp
public sealed record FamilyCreatedEvent(
    FamilyId FamilyId,
    FamilyName Name,
    UserId CreatedByUserId,
    DateTime CreatedAt
) : INotification;
```

**Updated Event:**

```csharp
public sealed record FamilyNameUpdatedEvent(
    FamilyId FamilyId,
    FamilyName OldName,
    FamilyName NewName,
    DateTime UpdatedAt
) : INotification;
```

**Deleted Event:**

```csharp
public sealed record FamilyDeletedEvent(
    FamilyId FamilyId,
    UserId DeletedByUserId,
    DateTime DeletedAt
) : INotification;
```

## Validation

- [ ] Event defined as sealed record implementing INotification
- [ ] Event raised in aggregate factory/method
- [ ] Event handler created
- [ ] Handler registered via DI (automatic with MediatR)
