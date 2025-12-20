# ADR-001: Start with Modular Monolith, Extract to Microservices Later

**Date**: 2025-12-20
**Status**: ✅ RECOMMENDED
**Deciders**: Architecture Review Team
**Context**: Issue #11 Architecture Review

---

## Context

Family Hub was originally designed as 8 microservices from day one, deployed on Kubernetes with GraphQL schema stitching. While this architecture is sound for a team, **it's over-engineered for a single developer** with AI assistance.

**The Problem**:
- 8 microservices + API Gateway + Event Bus = 10+ deployments
- Kubernetes operational overhead = 30-40% of development time
- Distributed debugging is 5-10x harder than monolith
- GraphQL schema stitching adds complexity
- High risk of developer burnout (CRITICAL risk)

**Key Question**: How can we maintain the excellent domain boundaries while reducing operational complexity?

---

## Decision

**WE WILL start with a Modular Monolith architecture in Phase 1-4, then extract to microservices in Phase 5+ using the Strangler Fig pattern.**

### Modular Monolith Structure

```
family-hub-api/ (Single .NET 8 Project)
├── Modules/
│   ├── Auth/
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── API/ (Controllers or GraphQL types)
│   ├── Calendar/
│   ├── Tasks/
│   ├── Shopping/
│   ├── Health/
│   ├── MealPlanning/
│   ├── Finance/
│   └── Communication/
└── Shared/
    ├── EventBus/ (In-process or RabbitMQ)
    ├── Database/ (PostgreSQL with RLS)
    └── API/ (REST → GraphQL later)
```

### Key Principles

1. **Clear Module Boundaries**: Each module is a DDD bounded context
2. **No Cross-Module Database Access**: Modules access only their own schemas
3. **Event-Driven Integration**: Modules communicate via events (in-process initially)
4. **Interface-Based Communication**: Easy to extract to HTTP later
5. **Database-Per-Module**: Separate PostgreSQL schemas (same as microservices plan)

---

## Rationale

### Comparison Analysis

| Criterion | Modular Monolith | Microservices | Winner |
|-----------|------------------|---------------|--------|
| **Time to MVP** | 10-14 months | 16-22 months | ✅ **Monolith (-6-12 months)** |
| **Development Hours** | 820-960 hours | 1,020-1,160 hours | ✅ **Monolith (-200 hours)** |
| **Debugging Complexity** | 1x (baseline) | 10x (distributed) | ✅ **Monolith (10x easier)** |
| **Deployment Complexity** | 1 deployment | 8+ deployments | ✅ **Monolith (8x simpler)** |
| **Infrastructure Cost** | $40-100/month | $195-400/month | ✅ **Monolith (-$95-300/month)** |
| **Operational Overhead** | 10% dev time | 40% dev time | ✅ **Monolith (-30%)** |
| **Scalability Limit** | 1,000-5,000 families | 100,000+ families | ❌ **Microservices** |
| **Technology Flexibility** | Single stack | Polyglot possible | ❌ **Microservices** |

**Verdict**: Modular Monolith wins on 6/8 criteria for single developer with AI assistance

### Why This Matters for Single Developer

**Developer Time Budget**: 20 hours/week = ~1,000 hours/year

**Microservices Architecture**:
```
Development: 15 hours/week (CI/CD overhead)
Debugging: 5 hours/week (distributed tracing, logs)
Deployment: 2 hours/week (8 services × 15 min)
Monitoring: 3 hours/week (8 dashboards)
Incident Response: 4 hours/week (finding failing service)
-------------------------------------------------
Total: 29 hours/week (9 hours OVER budget)
```

**Modular Monolith**:
```
Development: 20 hours/week (single codebase)
Debugging: 2 hours/week (F5, breakpoints work)
Deployment: 0.5 hours/week (single deployment)
Monitoring: 1 hour/week (single dashboard)
Incident Response: 1 hour/week (easier to find issues)
-------------------------------------------------
Total: 24.5 hours/week (WITHIN budget)
```

**Impact**: -4.5 hours/week = -18 hours/month = -216 hours/year saved

### Maintaining Microservices Benefits

The modular monolith **preserves the microservices migration path**:

1. **Same Domain Boundaries**: Modules match future microservices exactly
2. **Same Database Strategy**: Separate schemas (already multi-tenant with RLS)
3. **Same Event-Driven Architecture**: Events work in-process or over network
4. **Clean Interfaces**: Easy to replace in-process calls with HTTP calls

**Migration Effort**: 2-3 weeks per service extraction (vs 6-8 months building all microservices from scratch)

---

## Consequences

### Positive

✅ **Faster Time to Market**: 10-14 months vs 16-22 months (-6-12 months)
✅ **Lower Infrastructure Cost**: $40-100/month vs $195-400/month (60% savings)
✅ **Simpler Debugging**: Single codebase, breakpoints work, stack traces complete
✅ **Reduced Burnout Risk**: From CRITICAL to MEDIUM (major improvement)
✅ **Easier Testing**: No network calls, faster test execution
✅ **Single Deployment**: Atomic deploys, easier rollbacks
✅ **Better AI Assistance**: Claude Code generates 80% vs 60% (simpler context)

### Negative

❌ **Scalability Limit**: Vertical scaling only (1,000-5,000 families max)
❌ **Single Point of Failure**: Entire app down if process crashes
❌ **Resource Limits**: Cannot scale components independently
❌ **Team Scalability**: Harder for multiple teams (not relevant for solo dev)
❌ **Technology Lock-In**: Cannot use different languages per service

### Mitigation Strategies

**For Scalability Limit**:
- **Threshold**: Extract to microservices when hitting 1,000 families (Phase 5)
- **Monitoring**: Track performance metrics, set alerts at 70% capacity
- **Preparation**: Maintain clean module boundaries for easy extraction

**For Single Point of Failure**:
- **High Availability**: Run 2 instances behind load balancer (Phase 3+)
- **Health Checks**: Liveness and readiness probes
- **Auto-Restart**: Docker/Kubernetes auto-restart on crash

**For Resource Limits**:
- **Vertical Scaling**: Can handle 5,000 families with 8-16GB RAM
- **Profiling**: Identify bottlenecks early
- **Extract Early**: If one module hits limits, extract it first

---

## Migration Plan (Strangler Fig Pattern)

### Phase 1-4: Pure Modular Monolith (Months 1-9)

```
family-hub-api (100% monolith)
  └── All 8 modules in single deployment
  └── Docker Compose for deployment
  └── Target: 100-1,000 families
```

### Phase 5: Extract Calendar + Task Services (Months 10-12)

```
Step 1: Extract Calendar Module
  1. Create new repo: FamilyHub.CalendarService
  2. Copy Modules/Calendar/* to new repo
  3. Add REST API endpoints
  4. Deploy as separate service (Docker container)
  5. Update monolith to call Calendar Service via HTTP
  6. Run in parallel (monolith + microservice) for 2 weeks
  7. Switch traffic to microservice
  8. Remove Calendar module from monolith

Step 2: Repeat for Task Service

Architecture After Phase 5 (30% extracted):
  ├── Calendar Service (microservice)
  ├── Task Service (microservice)
  └── family-hub-api (70% monolith)
      ├── Auth, Shopping, Health
      ├── MealPlanning, Finance, Communication

Target: 1,000-5,000 families
```

### Phase 6: Extract Shopping + Health (Months 13-15)

```
Architecture After Phase 6 (60% extracted):
  ├── Calendar Service
  ├── Task Service
  ├── Shopping Service
  ├── Health Service
  └── family-hub-api (40% monolith)
      ├── Auth, MealPlanning
      ├── Finance, Communication

Target: 5,000-10,000 families
```

### Phase 7: Complete Extraction (Months 16-18)

```
Architecture After Phase 7 (100% microservices):
  ├── Auth Service
  ├── Calendar Service
  ├── Task Service
  ├── Shopping Service
  ├── Health Service
  ├── MealPlanning Service
  ├── Finance Service
  └── Communication Service

Retire monolith entirely

Target: 10,000+ families
```

---

## Implementation Details

### Module Registration Pattern

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register modules (order matters for dependencies)
builder.Services.AddModule<AuthModule>();
builder.Services.AddModule<CalendarModule>();
builder.Services.AddModule<TaskModule>();
builder.Services.AddModule<ShoppingModule>();
builder.Services.AddModule<HealthModule>();
builder.Services.AddModule<MealPlanningModule>();
builder.Services.AddModule<FinanceModule>();
builder.Services.AddModule<CommunicationModule>();

var app = builder.Build();

// Configure middleware
app.UseModules();

app.Run();
```

### Module Interface

```csharp
public interface IModule
{
    void RegisterServices(IServiceCollection services, IConfiguration config);
    void ConfigureApp(WebApplication app);
}

public class CalendarModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        // Domain services
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<ICalendarRepository, CalendarRepository>();

        // Event handlers
        services.AddEventHandler<HealthAppointmentScheduledEvent, CalendarEventHandler>();

        // GraphQL (if using GraphQL)
        services.AddGraphQLServer()
            .AddQueryType<CalendarQueries>()
            .AddMutationType<CalendarMutations>();
    }

    public void ConfigureApp(WebApplication app)
    {
        // Map REST endpoints
        app.MapGroup("/api/calendar")
            .MapCalendarEndpoints()
            .RequireAuthorization();

        // Or map GraphQL
        app.MapGraphQL("/graphql/calendar");
    }
}
```

### Event Bus (In-Process)

```csharp
public class InProcessEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;

    public async Task PublishAsync<TEvent>(TEvent domainEvent)
        where TEvent : DomainEvent
    {
        using var scope = _serviceProvider.CreateScope();

        // Find all handlers for this event type
        var handlers = scope.ServiceProvider
            .GetServices<IEventHandler<TEvent>>();

        // Execute in parallel (all in same process)
        await Task.WhenAll(
            handlers.Select(h => h.HandleAsync(domainEvent))
        );
    }
}
```

### Module Communication

**Current (Modular Monolith)**:
```csharp
// In-process call via interface
public class HealthAppointmentService
{
    private readonly IEventBus _eventBus; // In-process

    public async Task ScheduleAppointmentAsync(Appointment apt)
    {
        await _repo.SaveAsync(apt);

        // Publish event (handled in-process)
        await _eventBus.PublishAsync(
            new HealthAppointmentScheduledEvent(apt)
        );
    }
}
```

**Future (Microservices)**:
```csharp
// HTTP call via client
public class HealthAppointmentService
{
    private readonly IEventBus _eventBus; // RabbitMQ

    public async Task ScheduleAppointmentAsync(Appointment apt)
    {
        await _repo.SaveAsync(apt);

        // Publish event (sent over network)
        await _eventBus.PublishAsync(
            new HealthAppointmentScheduledEvent(apt)
        );
    }
}
```

**Key**: Same code, different `IEventBus` implementation!

---

## Alternatives Considered

### Alternative 1: Microservices from Day 1 (Original Plan)

**Rejected** because:
- Too complex for single developer (40% time on ops)
- High risk of developer burnout (CRITICAL risk)
- Slower time to market (16-22 months)
- Higher infrastructure costs ($195+/month)

### Alternative 2: Pure Monolith (No Module Boundaries)

**Rejected** because:
- Harder to extract to microservices later
- Poor separation of concerns
- Difficult to test independently
- No clear service boundaries

### Alternative 3: Microservices for Core, Monolith for Rest

**Rejected** because:
- Complexity of managing both paradigms
- Unclear which services to split initially
- Better to validate product-market fit first

---

## Success Metrics

### Phase 1-4 Success Criteria (Modular Monolith)

- ✅ Single deployment to production
- ✅ <2s p95 API response time
- ✅ 100-1,000 families supported
- ✅ 95%+ uptime
- ✅ Module boundaries respected (no cross-module DB access)
- ✅ Event-driven integration working
- ✅ Developer happiness: HIGH (manageable complexity)

### Phase 5+ Success Criteria (Microservices Extraction)

- ✅ Zero-downtime migration
- ✅ No performance regression
- ✅ Independent deployment of extracted services
- ✅ 99%+ uptime maintained
- ✅ Support for 5,000+ families

---

## References

- [Architecture Review Report](ARCHITECTURE-REVIEW-REPORT.md)
- [Microservices Migration Plan](MICROSERVICES-MIGRATION-PLAN.md)
- [Simplification Recommendations](SIMPLIFICATION-RECOMMENDATIONS.md)
- Issue #11: Technical Architecture Review & Validation

---

## Decision Log

| Date | Status | Notes |
|------|--------|-------|
| 2025-12-20 | ✅ RECOMMENDED | Based on comprehensive architecture review |
| TBD | PENDING | Awaiting stakeholder approval |

**Approved By**: (Pending)
**Implemented By**: (Pending - Phase 0)

---

**Status**: ✅ RECOMMENDED - Awaiting approval
**Impact**: -200 development hours, -$1,500-2,000 infrastructure cost Year 1
**Risk Reduction**: Developer Burnout from CRITICAL → MEDIUM
