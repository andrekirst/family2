# Architecture Documentation Guide

**Purpose:** Guide to architectural decisions, domain model, event chains, and system design patterns in Family Hub.

**Key Resources:** 6 ADRs, Domain Model, Event Chains Reference, Multi-Tenancy Strategy

---

## Quick Reference

### Architecture Decision Records (ADRs)

Family Hub has **6 ADRs** documenting critical architectural decisions:

1. **[ADR-001: Modular Monolith First](ADR-001-MODULAR-MONOLITH-FIRST.md)** - Why not microservices from day one
2. **[ADR-002: OAuth with Zitadel](ADR-002-OAUTH-WITH-ZITADEL.md)** - OAuth provider selection
3. **[ADR-003: GraphQL Input/Command Pattern](ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)** - Presentation/domain separation
4. **[ADR-004: Playwright Migration](ADR-004-PLAYWRIGHT-MIGRATION.md)** - E2E testing framework
5. **[ADR-005: Family Module Extraction](ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md)** - Bounded context extraction
6. **[ADR-006: Email-Only Authentication](ADR-006-EMAIL-ONLY-AUTHENTICATION.md)** - Auth strategy

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

**Last Updated:** 2026-01-09
**Derived from:** Root CLAUDE.md v5.0.0
**Canonical Sources:**

- ADR-001 through ADR-006 (Architectural decisions)
- domain-model-microservices-map.md (DDD structure)
- event-chains-reference.md (Event automation)
- multi-tenancy-strategy.md (RLS implementation)

**Sync Checklist:**

- [ ] ADR summaries match full ADR documents
- [ ] Domain model diagram accurate
- [ ] Event chain examples match reference
- [ ] Module count and names correct (8 modules)
