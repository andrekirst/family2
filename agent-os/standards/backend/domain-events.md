# Domain Events

Use MediatR INotification for domain events. Publish via RabbitMQ for cross-module communication.

## Event Definition

```csharp
public sealed record FamilyCreatedEvent(
    FamilyId FamilyId,
    FamilyName Name,
    UserId OwnerId,
    DateTime CreatedAt
) : INotification;
```

## Publishing (in aggregate)

```csharp
public class Family : AggregateRoot
{
    public static Family Create(FamilyName name, UserId ownerId)
    {
        var family = new Family
        {
            Id = FamilyId.New(),
            Name = name,
            OwnerId = ownerId
        };

        family.AddDomainEvent(new FamilyCreatedEvent(
            family.Id,
            family.Name,
            family.OwnerId,
            DateTime.UtcNow
        ));

        return family;
    }
}
```

## Handler

```csharp
public sealed class SendWelcomeEmailHandler
    : INotificationHandler<FamilyCreatedEvent>
{
    public async Task Handle(
        FamilyCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        // Handle event
    }
}
```

## Cross-Module Events (RabbitMQ)

```csharp
await _messageBroker.PublishAsync(
    new FamilyCreatedEvent(family.Id, family.Name, ownerId, DateTime.UtcNow),
    cancellationToken);
```

## Rules

- Events are immutable records
- Location: `Domain/Events/{Name}Event.cs`
- Handlers: `Infrastructure/EventHandlers/{Name}Handler.cs`
- Use past tense: `Created`, `Updated`, `Deleted`
