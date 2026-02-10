---
name: domain-event
description: Create domain event with Wolverine handler
category: backend
module-aware: true
inputs:
  - eventName: PascalCase event name (e.g., InvitationSentEvent)
  - module: DDD module name
---

# Domain Event Skill

Create a domain event (sealed record extending DomainEvent) with a Wolverine handler.

## Event

Location: `Features/{Module}/Domain/Events/{EventName}.cs`

```csharp
public sealed record {EventName}(
    {EntityId} Id,
    DateTime CreatedAt
) : DomainEvent;
```

## Handler (Static Class)

Location: `Features/{Module}/Application/EventHandlers/{EventName}Handler.cs`

```csharp
public static class {EventName}Handler
{
    public static async Task Handle(
        {EventName} @event,
        I{Service} service,
        CancellationToken ct)
    {
        // Handle event
    }
}
```

## Publishing

In aggregate factory methods:

```csharp
RaiseDomainEvent(new {EventName}(entity.Id, DateTime.UtcNow));
```

## Validation

- [ ] Event is sealed record extending DomainEvent
- [ ] Handler is static class with static Handle()
- [ ] Uses past tense (Created, Sent, Accepted)
- [ ] Published via RaiseDomainEvent() on aggregate
