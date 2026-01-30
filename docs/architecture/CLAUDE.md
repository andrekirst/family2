# Architecture Documentation Guide

**Purpose:** Guide to architectural decisions, domain model, event chains, and system design patterns in Family Hub.

**Key Resources:** 13 ADRs, Domain Model, Event Chains Reference, Multi-Tenancy Strategy

---

## Quick Reference

### Architecture Decision Records (ADRs)

Family Hub has **13 ADRs** documenting critical architectural decisions:

**Core Architecture:**

1. **[ADR-001: Modular Monolith First](ADR-001-MODULAR-MONOLITH-FIRST.md)** - Why not microservices from day one
2. **[ADR-002: OAuth with Zitadel](ADR-002-OAUTH-WITH-ZITADEL.md)** - OAuth provider selection
3. **[ADR-003: GraphQL Input/Command Pattern](ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)** - Presentation/domain separation
4. **[ADR-004: Playwright Migration](ADR-004-PLAYWRIGHT-MIGRATION.md)** - E2E testing framework
5. **[ADR-005: Family Module Extraction](ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md)** - Bounded context extraction
6. **[ADR-006: Email-Only Authentication](ADR-006-EMAIL-ONLY-AUTHENTICATION.md)** - Auth strategy

**Infrastructure & Patterns:**

1. **[ADR-007: Family DbContext Separation](ADR-007-FAMILY-DBCONTEXT-SEPARATION-STRATEGY.md)** - One DbContext per module with schema separation
2. **[ADR-008: RabbitMQ Integration](ADR-008-RABBITMQ-INTEGRATION-STRATEGY.md)** - Message broker with Polly v8 resilience
3. **[ADR-009: Modular Middleware Composition](ADR-009-MODULAR-MIDDLEWARE-COMPOSITION-PATTERN.md)** - UseAuthModule/UseFamilyModule pattern
4. **[ADR-010: Performance Testing](ADR-010-PERFORMANCE-TESTING-STRATEGY.md)** - k6-based performance testing
5. **[ADR-011: DataLoader Pattern](ADR-011-DATALOADER-PATTERN.md)** - Hot Chocolate DataLoader for N+1 prevention
6. **[ADR-012: Architecture Testing](ADR-012-ARCHITECTURE-TESTING-STRATEGY.md)** - NetArchTest for architecture validation
7. **[ADR-013: GraphQL Schema Refactoring](ADR-013-GRAPHQL-SCHEMA-REFACTORING.md)** - Nested namespaces, Relay patterns, unified errors

---

## Critical Patterns (4)

### 1. Domain-Driven Design (8 Modules)

**Bounded Contexts:**

```
┌─────────────────────────────────────────────┐
│           Family Hub Platform                │
├─────────────────────────────────────────────┤
│                                             │
│  ┌──────────┐         ┌──────────┐          │
│  │   Auth   │────────▶│  Family  │          │
│  │ (Zitadel)│         │  Context │          │
│  └──────────┘         └──────────┘          │
│       │                     │               │
│       ▼                     ▼               │
│  ┌──────────┐         ┌──────────┐          │
│  │ Calendar │◀───────▶│   Task   │          │
│  └──────────┘         └──────────┘          │
│       │                     │               │
│       ▼                     ▼               │
│  ┌──────────┐         ┌──────────┐          │
│  │  Health  │────────▶│ Shopping │          │
│  └──────────┘         └──────────┘          │
│       │                     │               │
│       ▼                     ▼               │
│  ┌──────────┐         ┌──────────┐          │
│  │   Meal   │◀───────▶│ Finance  │          │
│  │ Planning │         │          │          │
│  └──────────┘         └──────────┘          │
│                                             │
│  ┌─────────────────────────────────┐        │
│  │   Communication (Notifications) │        │
│  └─────────────────────────────────┘        │
│                                             │
│  ┌─────────────────────────────────┐        │
│  │    Event Bus (RabbitMQ/Redis)   │        │
│  └─────────────────────────────────┘        │
└─────────────────────────────────────────────┘
```

**Module Ownership:**

| Module | Core Aggregates | Domain Events | Database Schema |
|--------|----------------|---------------|-----------------|
| **Auth** | User, Family | UserRegistered, FamilyCreated | auth |
| **Calendar** | Event, Appointment | EventCreated, AppointmentScheduled | calendar |
| **Task** | Task, Assignment | TaskCreated, TaskCompleted | task |
| **Shopping** | ShoppingList, Item | ListCreated, ItemAdded | shopping |
| **Health** | Appointment, Prescription | AppointmentScheduled, PrescriptionIssued | health |
| **Meal Planning** | MealPlan, Recipe | MealPlanCreated | meal |
| **Finance** | Budget, Expense | ExpenseTracked, BudgetExceeded | finance |
| **Communication** | Notification, Message | NotificationSent | communication |

**Full Specification:** [domain-model-microservices-map.md](domain-model-microservices-map.md)

---

### 2. Event Chain Automation

**Family Hub's flagship differentiator:** Automated cross-domain workflows that save 10-30 minutes per action.

**Example: Doctor Appointment Chain**

```
1. User schedules doctor appointment (Health Service)
   └─▶ DoctorAppointmentScheduledEvent
        │
        ├─▶ Calendar Service
        │   └─▶ Creates calendar event
        │        └─▶ CalendarEventCreatedEvent
        │
        ├─▶ Task Service
        │   └─▶ Creates preparation task ("Prepare questions for doctor")
        │        └─▶ TaskCreatedEvent
        │
        └─▶ Communication Service
            └─▶ Schedules reminder 24h before
                 └─▶ NotificationScheduledEvent

2. Doctor issues prescription (Health Service)
   └─▶ PrescriptionIssuedEvent
        │
        ├─▶ Shopping Service
        │   └─▶ Adds medication to shopping list
        │        └─▶ ShoppingItemAddedEvent
        │
        ├─▶ Task Service
        │   └─▶ Creates task "Pick up prescription"
        │        └─▶ TaskCreatedEvent
        │
        └─▶ Health Service
            └─▶ Schedules refill reminder
                 └─▶ RefillReminderScheduledEvent
```

**10 Event Chains Documented:** [event-chains-reference.md](event-chains-reference.md)

**Event-Driven Benefits:**

- **Loose coupling** - Services don't know about downstream consumers
- **Extensibility** - Add new handlers without modifying existing code
- **Resilience** - Event failures don't cascade
- **Auditability** - Full event log for compliance

---

### 3. Modular Monolith → Microservices Evolution

**Phase 0-4 (Current):** Modular Monolith

- Single deployment
- Shared database (PostgreSQL) with schema separation
- In-process event bus (MediatR)
- Clear module boundaries (prepare for extraction)

**Phase 5+ (Future):** Microservices

- Separate deployments per service
- Database per service
- Network event bus (RabbitMQ)
- Kubernetes orchestration
- API Gateway (NGINX/Kong)

**Migration Strategy:** Strangler Fig Pattern

1. Extract service as separate deployment
2. Route traffic to new service
3. Remove code from monolith
4. Repeat for next service

**See:** [ADR-001-MODULAR-MONOLITH-FIRST.md](ADR-001-MODULAR-MONOLITH-FIRST.md) for rationale.

---

### 4. Multi-Tenancy with Row-Level Security

**PostgreSQL Row-Level Security (RLS)** enforces data isolation at database level.

**Implementation:**

```sql
-- Enable RLS on table
ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

-- Create policy (user can only see their own data)
CREATE POLICY user_isolation_policy ON auth.users
    USING (id = current_setting('app.current_user_id', true)::uuid);

-- Create policy (user can see their family members)
CREATE POLICY family_isolation_policy ON auth.users
    USING (family_id = current_setting('app.current_family_id', true)::uuid);
```

**Benefits:**

- **Defense in depth** - Database enforces isolation even if app logic fails
- **GDPR compliance** - Strong data isolation
- **Performance** - Database-level filtering
- **Auditability** - Cannot bypass RLS policies

**See:** [multi-tenancy-strategy.md](multi-tenancy-strategy.md)

---

## ADR Summaries

### ADR-001: Modular Monolith First

**Decision:** Start with modular monolith, extract microservices in Phase 5+.

**Rationale:**

- Single developer (simpler operations)
- Faster initial development
- Clear module boundaries prepare for extraction
- Defer operational complexity until proven need

**Status:** Implemented

---

### ADR-002: OAuth with Zitadel

**Decision:** Use Zitadel for OAuth 2.0 / OIDC.

**Rationale:**

- Open-source, self-hostable
- Built-in multi-tenancy
- GDPR-compliant by design
- No vendor lock-in

**Alternatives Considered:** Auth0, Keycloak, Custom

**Status:** Implemented (Phase 0)

---

### ADR-003: GraphQL Input/Command Pattern

**Decision:** Separate GraphQL Input DTOs (primitives) from MediatR Commands (Vogen).

**Rationale:**

- Hot Chocolate cannot deserialize Vogen value objects
- Clean separation of presentation and domain
- Explicit validation at boundary

**Status:** Implemented (standard pattern)

---

### ADR-004: Playwright Migration

**Decision:** Migrate E2E tests from Cypress to Playwright.

**Rationale:**

- Multi-browser support (Chromium, Firefox, WebKit)
- Native TypeScript support
- API testing built-in
- Zero-retry policy enforced

**Status:** Implemented (January 2026)

---

### ADR-005: Family Module Extraction

**Decision:** Extract Family bounded context from Auth module following 4-phase pattern.

**Rationale:**

- Clear domain boundary (Family vs Auth)
- Reusable extraction pattern
- Preparation for microservices

**Status:** Implemented (Phase 1)

---

### ADR-006: Email-Only Authentication

**Decision:** Use email-only authentication (no username/password).

**Rationale:**

- Simpler UX (one field)
- No password management burden
- Magic link flow secure with PKCE
- Aligns with OAuth best practices

**Status:** Planned (Phase 1)

---

### ADR-007: Family DbContext Separation

**Decision:** One DbContext per module with PostgreSQL schema separation.

**Rationale:**

- Enforces bounded context boundaries at database level
- Cross-module ID references without FK constraints
- IUserLookupService for cross-module queries
- Pooled DbContext factories for DataLoader performance

**Status:** Implemented

---

### ADR-008: RabbitMQ Integration

**Decision:** IMessageBrokerPublisher abstraction with RabbitMqPublisher implementation.

**Rationale:**

- Polly v8 resilience (exponential backoff + jitter)
- Publisher confirms for guaranteed delivery
- Dead Letter Exchange for failed messages
- Testable through interface abstraction

**Status:** Implemented

---

### ADR-009: Modular Middleware Composition

**Decision:** UseAuthModule(), UseFamilyModule() extension methods for middleware.

**Rationale:**

- PostgresRlsContextMiddleware sets RLS session variables
- Explicit middleware ordering in Program.cs
- Module-specific middleware encapsulation
- Transaction-scoped session variables for fail-secure behavior

**Status:** Implemented

---

### ADR-010: Performance Testing

**Decision:** k6-based performance testing with custom DataLoader metrics.

**Rationale:**

- Validates DataLoader efficiency (p95 < 200ms)
- Custom metrics (dl_*) for granular analysis
- Configurable threshold sets (default, strict, stress, dataLoader)
- X-Test-User-Id header for test environment auth bypass

**Status:** Implemented

---

### ADR-011: DataLoader Pattern

**Decision:** Hot Chocolate GreenDonut DataLoaders for N+1 prevention.

**Rationale:**

- Query count: 201 → ≤3 (67x reduction)
- Latency: p95 < 200ms (42x improvement)
- Request-scoped caching via DataLoaderScope
- Automatic batching across GraphQL resolvers

**Status:** Implemented

---

### ADR-012: Architecture Testing

**Decision:** NetArchTest.Rules with ExceptionRegistry pattern.

**Rationale:**

- Automated Clean Architecture layer validation
- Module boundary separation enforcement
- ExceptionRegistry for known violations with phase tracking
- Negative test fixtures validate test correctness

**Status:** Implemented

---

### ADR-013: GraphQL Schema Refactoring

**Decision:** Adopt nested namespaces, Relay patterns, unified error handling, and entity-centric subscriptions.

**Rationale:**

- Nested namespaces organize queries/mutations by domain (auth, account, family)
- Relay Node interface enables global ID resolution and cache normalization
- HotChocolate mutation conventions unify error handling across all mutations
- Entity-centric subscriptions (`nodeChanged(id)`) provide flexible real-time updates

**Status:** Implemented (January 2026)

---

## When to Create New ADR

Create an ADR when making decisions about:

- **Infrastructure** - Cloud providers, databases, message brokers
- **Architecture** - Service boundaries, communication patterns
- **Security** - Authentication, authorization, encryption
- **Testing** - Framework choices, testing strategies
- **Development** - Build tools, CI/CD, development workflows

**ADR Template:**

```markdown
# ADR-XXX: Decision Title

**Status:** Proposed | Accepted | Deprecated | Superseded
**Date:** YYYY-MM-DD
**Decision Makers:** [Names]
**Related ADRs:** [ADR-XXX]

## Context
[Describe the problem and constraints]

## Decision
[State the decision clearly]

## Rationale
[Explain why this decision was made]

## Alternatives Considered
[List alternatives and why they were rejected]

## Consequences
[Positive and negative impacts]

## Implementation Notes
[How to implement this decision]

## References
[Links to relevant documentation]
```

---

## Related Documentation

- **Development:** [../development/CLAUDE.md](../development/CLAUDE.md) - Coding patterns
- **Backend:** [../../src/api/CLAUDE.md](../../src/api/CLAUDE.md) - Backend guide
- **Database:** [../../database/CLAUDE.md](../../database/CLAUDE.md) - Database patterns
- **Security:** [../security/CLAUDE.md](../security/CLAUDE.md) - Security patterns

---

**Last Updated:** 2026-01-12
**Derived from:** Root CLAUDE.md v5.0.0
**Canonical Sources:**

- ADR-001 through ADR-012 (Architectural decisions)
- domain-model-microservices-map.md (DDD structure)
- event-chains-reference.md (Event automation)
- multi-tenancy-strategy.md (RLS implementation)

**Sync Checklist:**

- [ ] ADR summaries match full ADR documents
- [ ] Domain model diagram accurate
- [ ] Event chain examples match reference
- [ ] Module count and names correct (8 modules)
- [ ] All 13 ADRs listed with correct links
