# ADR-008: RabbitMQ Integration Strategy

**Status:** Accepted
**Date:** 2026-01-12
**Deciders:** Andre Kirst (with Claude Code AI)
**Tags:** messaging, rabbitmq, polly, resilience, event-driven, infrastructure
**Related ADRs:** [ADR-001](ADR-001-MODULAR-MONOLITH-FIRST.md)
**Issue:** #76

## Context

Family Hub is built as an **event-driven modular monolith** (per [ADR-001](ADR-001-MODULAR-MONOLITH-FIRST.md)) where the primary differentiating feature is **event chain automation**—automated cross-domain workflows triggered by domain events. This requires a reliable message broker for asynchronous event delivery.

### Problem Statement

The application needs to:

1. **Publish domain events** from aggregates across multiple modules
2. **Guarantee message delivery** even during infrastructure failures
3. **Handle transient failures** with appropriate retry strategies
4. **Route failed messages** to dead letter queues for analysis
5. **Support future microservices** migration (Phase 5+)
6. **Enable health monitoring** for Kubernetes readiness probes

### Technology Context

- **.NET 10 / C# 14**: Target framework
- **RabbitMQ 3.12+**: Message broker
- **Polly v8**: Resilience and transient fault handling
- **Docker Compose**: Local development
- **Kubernetes**: Production deployment (Phase 5+)

### Event Chain Example

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ EVENT CHAIN: Doctor Appointment Automation                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ 1. Health Module                                                            │
│    └─▶ DoctorAppointmentScheduledEvent                                      │
│         │                                                                   │
│         ├─▶ Calendar Module → Creates calendar event                        │
│         ├─▶ Task Module → Creates preparation task                          │
│         └─▶ Communication Module → Schedules reminder                       │
│                                                                             │
│ 2. Health Module                                                            │
│    └─▶ PrescriptionIssuedEvent                                              │
│         │                                                                   │
│         ├─▶ Shopping Module → Adds medication to list                       │
│         └─▶ Task Module → Creates pickup task                               │
│                                                                             │
│ RESULT: One action triggers 5+ automated follow-ups (saves 10-30 min)       │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Decision

**Implement RabbitMQ integration using an abstracted `IMessageBrokerPublisher` interface with a production-grade `RabbitMqPublisher` implementation featuring Polly v8 resilience, connection pooling, publisher confirms, and dead letter queue support.**

### Interface Design

```csharp
/// <summary>
/// Interface for publishing messages to a message broker.
/// Abstracts broker implementation to enable easy swapping
/// (RabbitMQ, Azure Service Bus, Kafka, etc.)
/// </summary>
public interface IMessageBrokerPublisher
{
    /// <summary>
    /// Publishes a raw JSON message to an exchange.
    /// </summary>
    Task PublishAsync(
        string exchange,
        string routingKey,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a strongly-typed message (auto-serialized to JSON).
    /// </summary>
    Task PublishAsync<TMessage>(
        string exchange,
        string routingKey,
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : class;
}
```

### Resilience Pipeline (Polly v8)

```csharp
private ResiliencePipeline CreateRetryPipeline()
{
    return new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder()
                .Handle<BrokerUnreachableException>()
                .Handle<AlreadyClosedException>()
                .Handle<OperationInterruptedException>()
                .Handle<TimeoutException>()
                .Handle<IOException>(),
            MaxRetryAttempts = _settings.MaxRetryAttempts,  // Default: 3
            Delay = _settings.RetryBaseDelay,               // Default: 1s
            MaxDelay = _settings.RetryMaxDelay,             // Default: 30s
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,  // Prevents thundering herd
            OnRetry = args =>
            {
                LogRetryAttempt(args.AttemptNumber, args.RetryDelay, args.Outcome.Exception?.Message);
                return default;
            }
        })
        .Build();
}
```

### Connection Management

```csharp
/// <summary>
/// Thread-safe connection/channel management using double-checked locking.
/// </summary>
private async Task<IChannel> EnsureChannelAsync(CancellationToken cancellationToken)
{
    if (_channel is { IsOpen: true })
    {
        return _channel;
    }

    await _connectionLock.WaitAsync(cancellationToken);
    try
    {
        // Double-check after acquiring lock
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        // Create new connection
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            RequestedConnectionTimeout = _settings.ConnectionTimeout
        };

        _connection = await factory.CreateConnectionAsync(
            _settings.ClientProvidedName,
            cancellationToken);

        // Enable publisher confirms for guaranteed delivery
        var channelOptions = _settings.EnablePublisherConfirms
            ? new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true)
            : null;

        _channel = await _connection.CreateChannelAsync(
            options: channelOptions,
            cancellationToken: cancellationToken);

        return _channel;
    }
    finally
    {
        _connectionLock.Release();
    }
}
```

### Exchange Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ RabbitMQ Exchange Architecture                                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ ┌─────────────────────────────┐                                             │
│ │ family-hub.events           │ ◀── Main Event Exchange (Topic)             │
│ │ Type: Topic                 │                                             │
│ │ Durable: Yes                │                                             │
│ └─────────────────────────────┘                                             │
│          │                                                                  │
│          │ Routing Keys: "FamilyCreatedEvent", "UserRegisteredEvent", etc.  │
│          ▼                                                                  │
│    ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                        │
│    │ calendar.q  │  │  task.q     │  │ shopping.q  │                        │
│    │ (future)    │  │ (future)    │  │ (future)    │                        │
│    └─────────────┘  └─────────────┘  └─────────────┘                        │
│                                                                             │
│ ┌─────────────────────────────┐                                             │
│ │ family-hub.dlx              │ ◀── Dead Letter Exchange (Fanout)           │
│ │ Type: Fanout                │                                             │
│ │ Durable: Yes                │                                             │
│ └─────────────────────────────┘                                             │
│          │                                                                  │
│          ▼                                                                  │
│    ┌─────────────────────────┐                                              │
│    │ family-hub.dlq          │ ◀── Dead Letter Queue (all failed messages)  │
│    │ Durable: Yes            │                                              │
│    └─────────────────────────┘                                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Configuration

```csharp
public sealed class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";
    public string ClientProvidedName { get; init; } = "FamilyHub.Api";

    // Exchange configuration
    public string DefaultExchange { get; init; } = "family-hub.events";
    public string DeadLetterExchange { get; init; } = "family-hub.dlx";
    public string DeadLetterQueue { get; init; } = "family-hub.dlq";

    // Resilience configuration
    public int MaxRetryAttempts { get; init; } = 3;
    public TimeSpan RetryBaseDelay { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan RetryMaxDelay { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan ConnectionTimeout { get; init; } = TimeSpan.FromSeconds(30);

    // Reliability
    public bool EnablePublisherConfirms { get; init; } = true;
}
```

### Service Registration

```csharp
// Program.cs
builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddRabbitMqHealthCheck("rabbitmq", tags: ["ready", "infrastructure"]);

// RabbitMqServiceExtensions.cs
public static IServiceCollection AddRabbitMq(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.Configure<RabbitMqSettings>(
        configuration.GetSection(RabbitMqSettings.SectionName));

    // Singleton: manages its own connection lifecycle
    services.AddSingleton<IMessageBrokerPublisher, RabbitMqPublisher>();
    services.AddSingleton<RabbitMqHealthCheck>();

    return services;
}
```

## Rationale

### Why RabbitMQ

| Criteria | RabbitMQ | Azure Service Bus | Apache Kafka | Redis Streams |
|----------|----------|-------------------|--------------|---------------|
| **Hosting** | Self-hosted/Cloud | Cloud only | Self-hosted/Cloud | Self-hosted/Cloud |
| **Cost** | Open source | Pay per message | Open source | Open source |
| **AMQP Support** | Native | Partial | No | No |
| **Complexity** | Medium | Low | High | Low |
| **Message Patterns** | P2P, Pub/Sub, Routing | P2P, Pub/Sub | Pub/Sub, Streaming | Pub/Sub |
| **Durability** | Configurable | Built-in | Configurable | Configurable |
| **Family Hub Fit** | ✅ Best | ⚠️ Vendor lock-in | ❌ Overkill | ⚠️ Less mature |

**Decision**: RabbitMQ provides the best balance of features, cost, and flexibility for Family Hub's needs.

### Why Interface Abstraction

The `IMessageBrokerPublisher` interface enables:

1. **Testability**: Easy to mock in unit tests
2. **Swappability**: Can change to Azure Service Bus, Kafka, etc.
3. **Consistent API**: Same interface regardless of broker
4. **Dependency Injection**: Standard .NET DI patterns

### Why Polly v8 Resilience

**Exponential Backoff with Jitter**:

```
Attempt 1: Immediate
Attempt 2: ~1s delay (with jitter: 0.8s-1.2s)
Attempt 3: ~2s delay (with jitter: 1.6s-2.4s)
Attempt 4: ~4s delay (with jitter: 3.2s-4.8s)
```

**Jitter prevents thundering herd**: When multiple clients retry simultaneously after a broker restart, jitter spreads the retry attempts to prevent overwhelming the broker.

### Why Publisher Confirms

Without confirms:

```
Publisher ──▶ Broker ──▶ [Message may be lost]
```

With confirms:

```
Publisher ──▶ Broker ──▶ ACK ──▶ Publisher (confirmed)
                    └─▶ NACK ──▶ Publisher (retry)
```

**Publisher confirms ensure** the broker has accepted responsibility for the message before the publish operation completes.

### Why Dead Letter Queue

Failed messages are captured for:

1. **Debugging**: Investigate why messages failed
2. **Replay**: Manually reprocess after fixing issues
3. **Alerting**: Monitor DLQ depth for operational alerts
4. **Compliance**: Audit trail for failed operations

## Alternatives Considered

### Alternative 1: In-Process MediatR Only

**Approach**: Use MediatR notifications without external broker.

```csharp
// Events handled in same process
await _mediator.Publish(new FamilyCreatedEvent(family.Id), cancellationToken);
```

**Rejected Because**:

- No durability (events lost on crash)
- Cannot scale to multiple instances
- No dead letter handling
- Blocks microservices migration

### Alternative 2: Azure Service Bus

**Approach**: Use Azure-managed message broker.

```csharp
services.AddAzureServiceBus(configuration.GetConnectionString("ServiceBus"));
```

**Rejected Because**:

- Vendor lock-in to Azure
- Cost scales with message volume
- Less control over infrastructure
- Local development requires Azure subscription

### Alternative 3: Apache Kafka

**Approach**: Use Kafka for event streaming.

**Rejected Because**:

- Over-engineered for current scale
- Higher operational complexity
- Designed for streaming, not messaging
- Zookeeper dependency (pre-KRaft)

### Alternative 4: MassTransit

**Approach**: Use MassTransit abstraction over RabbitMQ.

```csharp
services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) => { ... });
});
```

**Deferred Because**:

- Additional abstraction layer
- Learning curve for team
- Direct RabbitMQ sufficient for current needs
- Can migrate to MassTransit later for consumers

## Consequences

### Positive

1. **Reliable Delivery**: Publisher confirms + retries ensure messages reach broker
2. **Resilience**: Polly handles transient failures gracefully
3. **Observability**: Health checks integrate with Kubernetes readiness
4. **Testability**: Interface abstraction enables easy mocking
5. **Future-Proof**: Abstraction allows broker swapping without code changes

### Negative

1. **Infrastructure Dependency**: Requires RabbitMQ running
2. **Eventual Consistency**: Async events mean eventual consistency
3. **Complexity**: More moving parts than in-process events
4. **Monitoring**: Requires broker monitoring (Management UI, Prometheus)

### Mitigation Strategies

| Risk | Mitigation |
|------|------------|
| Broker Unavailability | Polly retries + health checks + alerts |
| Message Loss | Publisher confirms + durable exchanges/queues |
| Debugging Difficulty | Structured logging + correlation IDs |
| Local Dev Setup | Docker Compose includes RabbitMQ |

## Implementation

### Files Created

| File | Purpose |
|------|---------|
| `FamilyHub.SharedKernel/Interfaces/IMessageBrokerPublisher.cs` | Abstraction interface |
| `FamilyHub.Infrastructure/Messaging/RabbitMqPublisher.cs` | Implementation |
| `FamilyHub.Infrastructure/Messaging/RabbitMqSettings.cs` | Configuration |
| `FamilyHub.Infrastructure/Messaging/RabbitMqServiceExtensions.cs` | DI registration |
| `FamilyHub.Infrastructure/Messaging/RabbitMqHealthCheck.cs` | Health check |

### Configuration (appsettings.json)

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "familyhub",
    "Password": "familyhub_secret",
    "VirtualHost": "/",
    "DefaultExchange": "family-hub.events",
    "DeadLetterExchange": "family-hub.dlx",
    "DeadLetterQueue": "family-hub.dlq",
    "MaxRetryAttempts": 3,
    "EnablePublisherConfirms": true
  }
}
```

### Docker Compose

```yaml
services:
  rabbitmq:
    image: rabbitmq:3.12-management
    ports:
      - "5672:5672"   # AMQP
      - "15672:15672" # Management UI
    environment:
      RABBITMQ_DEFAULT_USER: familyhub
      RABBITMQ_DEFAULT_PASS: familyhub_secret
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "check_running"]
      interval: 30s
      timeout: 10s
      retries: 5
```

### Verification

1. **Build**: `dotnet build` completes without errors
2. **Health Check**: `/health/ready` returns healthy when RabbitMQ available
3. **Message Flow**: Published events appear in RabbitMQ Management UI
4. **Resilience**: Retry logs appear when broker temporarily unavailable
5. **DLQ**: Failed messages routed to dead letter queue

## Related Decisions

- [ADR-001: Modular Monolith First](ADR-001-MODULAR-MONOLITH-FIRST.md) - Event-driven integration strategy

## Future Work

- **Consumers**: Implement message consumers with MassTransit (Phase 2+)
- **Outbox Pattern**: Transactional outbox for guaranteed event publishing
- **Saga Orchestration**: Long-running business processes
- **Monitoring**: Prometheus metrics export from RabbitMQ

## References

- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [Polly v8 Documentation](https://github.com/App-vNext/Polly)
- [.NET Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Event Chains Reference](event-chains-reference.md)

---

**Decision**: Implement RabbitMQ integration using an abstracted `IMessageBrokerPublisher` interface with Polly v8 resilience (exponential backoff + jitter), thread-safe connection pooling, publisher confirms, and dead letter queue support. This provides reliable event delivery for event chain automation while maintaining testability and future broker swappability.
