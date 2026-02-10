# References for Event Chain Engine

This document captures the code references studied during the exploration phase to inform the Event Chain Engine implementation.

---

## Core Domain Event Infrastructure

### 1. IDomainEvent Interface

**Location**: `src/FamilyHub.Api/Common/Domain/IDomainEvent.cs`

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
```

**Relevance**: All domain events implement this interface. The chain engine subscribes to events via this contract.

### 2. DomainEvent Base Record

**Location**: `src/FamilyHub.Api/Common/Domain/DomainEvent.cs`

```csharp
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
```

**Relevance**: Chain engine events (`ChainExecutionStartedEvent`, etc.) will extend this base record.

### 3. AggregateRoot Base Class

**Location**: `src/FamilyHub.Api/Common/Domain/AggregateRoot.cs`

```csharp
public abstract class AggregateRoot<TId> where TId : struct
{
    public TId Id { get; protected set; }
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) { ... }
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

**Relevance**: `ChainDefinition` and `ChainExecution` aggregates will inherit from `AggregateRoot<T>`. The `RaiseDomainEvent()` pattern is used for chain lifecycle events.

---

## Event Publishing Infrastructure

### 4. AppDbContext — Automatic Event Publishing

**Location**: `src/FamilyHub.Api/Common/Database/AppDbContext.cs`

**Key Implementation**:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    // 1. Collect domain events BEFORE saving
    var domainEvents = CollectDomainEventsFromAggregates();

    // 2. Save changes to database
    var result = await base.SaveChangesAsync(cancellationToken);

    // 3. Publish domain events AFTER successful save
    if (_messageBus is not null)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _messageBus.PublishAsync(domainEvent);
        }
    }

    // 4. Clear events from aggregates
    ClearDomainEventsFromAggregates();

    return result;
}
```

**Relevance**: This is the integration point for the chain engine. When domain events are published via `_messageBus.PublishAsync()`, the chain engine's Wolverine handler will receive them and check for matching chain definitions.

**Key Insight**: Events are published AFTER database commit. This means the chain engine is guaranteed that the triggering entity exists in the database before the chain starts executing.

---

## CQRS Infrastructure

### 5. ICommand + ICommandBus

**Location**: `src/FamilyHub.Api/Common/Application/ICommand.cs` and `ICommandBus.cs`

```csharp
public interface ICommand<out TResult> { }

public interface ICommandBus
{
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);
}
```

**Relevance**: Chain engine commands (`CreateChainDefinitionCommand`, `ExecuteChainCommand`) implement `ICommand<T>`. Handlers follow the Wolverine static class pattern.

### 6. WolverineCommandBus

**Location**: `src/FamilyHub.Api/Common/Infrastructure/Messaging/WolverineCommandBus.cs`

```csharp
public sealed class WolverineCommandBus(IMessageBus messageBus) : ICommandBus
{
    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        return await messageBus.InvokeAsync<TResult>(command, ct);
    }
}
```

**Relevance**: Chain engine uses the same command bus for its own commands and for dispatching action steps that invoke other module commands.

### 7. IQuery + IQueryBus

**Location**: `src/FamilyHub.Api/Common/Application/IQuery.cs` and `IQueryBus.cs`

```csharp
public interface IQuery<out TResult> { }

public interface IQueryBus
{
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default);
}
```

**Relevance**: Chain definition queries (`GetChainDefinitionsQuery`, `GetChainExecutionQuery`) implement `IQuery<T>`.

---

## Example Domain Events

### 8. UserRegisteredEvent

**Location**: `src/FamilyHub.Api/Features/Auth/Domain/Events/UserRegisteredEvent.cs`

```csharp
public sealed record UserRegisteredEvent(
    UserId UserId,
    Email Email,
    UserName Name,
    ExternalUserId ExternalUserId,
    bool EmailVerified,
    DateTime RegisteredAt
) : DomainEvent;
```

**Relevance**: Example of a domain event that could be a chain trigger. Shows the pattern: sealed record with Vogen value objects, extending DomainEvent.

### 9. FamilyCreatedEvent

**Location**: `src/FamilyHub.Api/Features/Family/Domain/Events/FamilyCreatedEvent.cs`

```csharp
public sealed record FamilyCreatedEvent(
    FamilyId FamilyId,
    FamilyName FamilyName,
    UserId OwnerId,
    DateTime CreatedAt
) : DomainEvent;
```

**Relevance**: Another trigger candidate. Chain engine will be able to listen for any `DomainEvent` subtype.

### 10. UserFamilyAssignedEvent / UserFamilyRemovedEvent

**Location**: `src/FamilyHub.Api/Features/Auth/Domain/Events/`

**Relevance**: These show cross-domain events (Auth publishes, other modules consume). Chain engine operates in this same pattern.

---

## Example Wolverine Handlers

### 11. UserRegisteredEventHandler

**Location**: `src/FamilyHub.Api/Features/Auth/Application/EventHandlers/UserRegisteredEventHandler.cs`

```csharp
public static class UserRegisteredEventHandler
{
    public static Task Handle(
        UserRegisteredEvent @event,
        ILogger logger)
    {
        logger.LogInformation(
            "User registered: UserId={UserId}, Email={Email}, Name={Name}",
            @event.UserId.Value,
            @event.Email.Value,
            @event.Name.Value);
        return Task.CompletedTask;
    }
}
```

**Relevance**: Shows the Wolverine handler convention: static class + static `Handle` method. The chain engine's event listener (`ChainTriggerHandler`) will follow this exact pattern. Parameters are dependency-injected.

**Key Pattern**: The chain engine will have a handler like:

```csharp
public static class ChainTriggerHandler
{
    public static async Task Handle(
        IDomainEvent @event,
        IChainOrchestrator orchestrator,
        ILogger logger)
    {
        await orchestrator.TryTriggerChainsAsync(@event);
    }
}
```

---

## GraphQL Patterns

### 12. FamilyMutations — Input→Command Pattern

**Location**: `src/FamilyHub.Api/Features/Family/GraphQL/FamilyMutations.cs`

```csharp
[Authorize]
public async Task<FamilyDto> CreateFamily(
    CreateFamilyRequest input,
    ClaimsPrincipal claimsPrincipal,
    [Service] ICommandBus commandBus,
    ...)
{
    var familyName = FamilyName.From(input.Name.Trim());
    var command = new CreateFamilyCommand(familyName, user.Id);
    var result = await commandBus.SendAsync<CreateFamilyResult>(command, ct);
    return await GetFamilyDto(result.FamilyId);
}
```

**Relevance**: Chain engine mutations will follow this exact pattern. `CreateChainDefinitionRequest` (primitives) → `CreateChainDefinitionCommand` (Vogen) → handler.

### 13. FamilyQueries — GraphQL Query Extension

**Location**: `src/FamilyHub.Api/Features/Family/GraphQL/FamilyQueries.cs`

**Relevance**: Chain engine queries registered via `.AddTypeExtension<ChainQueries>()` in Program.cs.

---

## Repository Pattern

### 14. IFamilyRepository + FamilyRepository

**Location**: `src/FamilyHub.Api/Features/Family/Domain/Repositories/IFamilyRepository.cs` and `Infrastructure/Repositories/FamilyRepository.cs`

**Relevance**: Chain engine repositories (`IChainDefinitionRepository`, `IChainExecutionRepository`) follow the same interface + implementation pattern. Primary constructor with `AppDbContext`.

---

## EF Core Configuration Pattern

### 15. FamilyConfiguration

**Location**: `src/FamilyHub.Api/Features/Family/Data/FamilyConfiguration.cs`

**Relevance**: Chain engine entity configurations follow this pattern. Manual `.HasConversion()` for Vogen types, schema separation, index definitions.

---

## RLS Migration Pattern

### 16. AddRlsPolicies Migration

**Location**: `src/FamilyHub.Api/Migrations/AddRlsPolicies.cs`

**Relevance**: Chain engine RLS migration follows this pattern. All 6 tables in `event_chain` schema get family isolation policies.

---

## Documentation References

### Architecture Documents

- `docs/architecture/event-chains-reference.md` — 11 documented chains with full event flows, expected outcomes, time savings
- `docs/architecture/domain-model-microservices-map.md` — 8 bounded contexts, all aggregates and events
- `docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md` — Modular monolith strategy
- `docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md` — Input→Command separation

### Standards

- `agent-os/standards/backend/domain-events.md`
- `agent-os/standards/architecture/event-chains.md`
- `agent-os/standards/architecture/ddd-modules.md`
- `agent-os/standards/database/ef-core-migrations.md`
- `agent-os/standards/database/rls-policies.md`
- `agent-os/standards/backend/graphql-input-command.md`
- `agent-os/standards/backend/vogen-value-objects.md`
- `agent-os/standards/testing/unit-testing.md`

---

## Key Architectural Insights

### 1. Wolverine Is the Bus

The codebase uses Wolverine (not MediatR) for both command handling and event publishing. The chain engine integrates at the Wolverine level, receiving events via the same `IMessageBus` pipeline.

### 2. Events Published After Commit

`AppDbContext.SaveChangesAsync()` publishes events AFTER the database transaction succeeds. This guarantees the chain engine will only process events for entities that actually exist.

### 3. Static Handler Convention

All Wolverine handlers are static classes with static `Handle()` methods. Dependencies are injected as method parameters. The chain engine's trigger handler follows this exact convention.

### 4. Dual Dispatch Is Natural

Wolverine already supports multiple handlers for the same event type. Adding a `ChainTriggerHandler` alongside existing handlers (like `UserRegisteredEventHandler`) requires no configuration changes — Wolverine discovers it automatically.

### 5. Single AppDbContext

The codebase currently uses a single `AppDbContext` (not per-module). The chain engine's entity configurations will be added to this context with the `event_chain` schema prefix.
