# Event Chain Engine — Shaping Notes

**Feature**: Saga-based event chain orchestration with user-defined custom chain builder
**Created**: 2026-02-09

---

## Scope

The Event Chain Engine is an orchestration layer built on top of Family Hub's existing domain event infrastructure. It turns individual domain events (like `HealthAppointmentScheduledEvent`) into coordinated multi-step workflows — automatically creating calendar events, tasks, notifications, and shopping list items across bounded contexts.

### What's In Scope (V1)

1. **Core Engine**: Saga orchestrator, state machine, plugin registry, middleware pipeline
2. **State Management**: PostgreSQL-backed saga state (chain + step executions)
3. **Scheduler**: PostgreSQL polling for delayed/scheduled steps
4. **Compensation**: Semantic undo for failed chains
5. **User-Defined Chains**: Full chain builder from trigger/action catalog
6. **GraphQL API**: Complete CRUD + subscriptions for chain management
7. **Frontend UI**: Chain builder, execution monitoring, template gallery
8. **V1 Templates**: Doctor Appointment, Task Assignment, Calendar Reminder
9. **Observability**: Correlation ID propagation, execution history

### What's Out of Scope (Future)

1. Distributed execution (cross-service saga) — V1 is in-process only
2. Parallel step execution — V1 is sequential steps only
3. Complex branching (switch/case) — V1 supports simple if/else per step
4. Chain versioning/migration — V1 uses strict action versioning only
5. Chain marketplace/sharing between families
6. Visual drag-and-drop chain builder — V1 uses form-based builder
7. AI-suggested chains based on usage patterns

---

## Decisions

### 1. Execution Model

**Question**: How should multi-step chains execute?

**Answer**: **Saga/Orchestrator Pattern**

- Each chain is a state machine with forward steps and compensation steps
- Central orchestrator manages state transitions
- Not choreography (event-driven) — we need explicit control flow and compensation

**Rationale**: Choreography doesn't provide compensation guarantees. Saga pattern gives us explicit state, retry capability, and deterministic failure handling.

### 2. Failure Semantics

**Question**: What happens when a step fails?

**Answer**: **Circuit Breaker + Partial Completion**

- Failed steps trigger circuit breaker for that action type
- Other steps continue executing (partial completion)
- Unhealthy steps are skipped until circuit closes
- Permanent failures escalate to user notification

**Rationale**: Families shouldn't lose their calendar event just because the notification service is temporarily down. Partial completion is better than all-or-nothing.

### 3. User Chains (V1)

**Question**: Can users create their own chains or only use pre-built templates?

**Answer**: **Full Custom Chains**

- Users build chains from a trigger/action catalog
- Catalog populated by modules registering with `IChainRegistry`
- Type-compatible validation prevents nonsensical chains
- V1 templates provided as starting points

**Rationale**: Custom chains are the long-term differentiator. Templates are just pre-configured custom chains. Building the custom infrastructure now means templates are trivial.

### 4. State Store

**Question**: Where is saga state persisted?

**Answer**: **PostgreSQL Tables**

- `chain_executions` — one row per chain run
- `step_executions` — one row per step in a chain run
- Dedicated `event_chain` schema
- Transaction-safe with existing EF Core infrastructure

**Rationale**: PostgreSQL is already our primary store. No need for a separate saga database. Row-level locking provides concurrent safety.

### 5. Compensation Strategy

**Question**: How do we undo completed steps when a later step fails?

**Answer**: **Semantic Undo**

- Actions declare whether they're compensatable
- Compensatable actions provide a reverse action (e.g., cancel calendar event)
- Non-compensatable actions send correction notifications instead
- User informed of partial state

**Rationale**: True rollback is impossible for some actions (you can't unsend a notification). Correction notifications are the honest approach.

### 6. Module Coupling

**Question**: How do modules register their triggers and actions?

**Answer**: **Plugin/Registry Pattern**

- Each module implements `IChainPlugin` at startup
- Registers triggers (events it publishes) and actions (operations it can perform)
- `IChainRegistry` is the central catalog
- Modules discovered via assembly scanning

**Rationale**: Keeps module boundaries clean. No circular dependencies. Modules don't know about the chain engine — they just register capabilities.

### 7. Scheduling

**Question**: How are delayed/scheduled steps executed?

**Answer**: **PostgreSQL Polling with SELECT FOR UPDATE SKIP LOCKED**

- Scheduler polls `step_executions` table for ready steps
- `SELECT FOR UPDATE SKIP LOCKED` prevents double-execution in multi-instance scenarios
- Polling interval configurable (default: 5 seconds)
- `picked_up_at` column for stale job detection

**Rationale**: PostgreSQL is already there. No need for Hangfire, Quartz, or external scheduler. Row locking is battle-tested for this pattern.

### 8. Chain Validation

**Question**: How do we prevent users from creating invalid chains?

**Answer**: **Type-Compatible Validation**

- Each action declares input/output types
- Chain builder validates that step N output is compatible with step N+1 input
- Prevents "send notification after delete calendar event" (no context to send)
- Validation runs at chain save time, not execution time

**Rationale**: Catch errors at design time, not runtime. Type compatibility is a natural constraint that users can understand.

### 9. Privacy Model

**Question**: Who can see chain definitions and executions?

**Answer**: **Role-Based with RLS**

- Parents/Admins: See all chains and executions for the family
- Members: See their own chains and executions
- Children: See simplified view (no execution details)
- RLS policies on all `event_chain` schema tables

**Rationale**: Consistent with Family Hub's privacy-first approach. RLS provides defense in depth.

### 10. Context Flow

**Question**: How do steps pass data to subsequent steps?

**Answer**: **Typed Step Outputs Namespaced by Alias**

- Each step produces typed output stored in `ChainExecutionContext`
- Outputs keyed by step **alias** (user-defined name), not position index
- Subsequent steps reference context as `{{steps.create_calendar.calendarEventId}}`
- Alias-based naming is stable across chain edits (adding/removing steps)

**Rationale**: Position-based context (`step[0].output`) breaks when users reorder steps. Aliases are stable and self-documenting.

### 11. Step Execution Pipeline

**Question**: How does each step execute?

**Answer**: **Custom Middleware Pipeline**

```
Logging → Metrics → CircuitBreaker → Retry → Compensation → ActionHandler
```

- Fixed global pipeline (all chains share the same middleware stack)
- Logging: Structured logs with correlation ID
- Metrics: Step duration, success rate, circuit state
- CircuitBreaker: Per-action-type health tracking
- Retry: Exponential backoff with jitter (3 attempts)
- Compensation: Triggers reverse action on failure
- ActionHandler: Executes the actual action

**Rationale**: Middleware pipeline is a proven pattern (ASP.NET Core, Express.js). Fixed pipeline avoids configuration complexity while maintaining extensibility.

### 12. Coexistence with Wolverine

**Question**: How does the chain engine coexist with existing Wolverine event handlers?

**Answer**: **Dual Dispatch**

- Domain events published via `IMessageBus` (existing behavior)
- Wolverine handlers continue to process events (existing behavior)
- Chain engine listens for the same events independently
- Both execute — Wolverine for module-specific logic, chain engine for cross-domain orchestration

**Rationale**: Chain engine doesn't replace existing event handlers. It's an additional orchestration layer. No migration needed for existing code.

### 13. Conditions

**Question**: Can steps be conditional?

**Answer**: **Simple If/Else Per Step**

- Each step can have an optional condition expression
- Condition evaluates against the chain execution context
- If condition is false, step is skipped (not failed)
- Expression language: simple property access + comparison operators

Example: `{{trigger.hasDeadline}} == true`

**Rationale**: Full expression languages (like JSONPath) are over-engineered for V1. Simple if/else covers 90% of use cases.

### 14. Action Versioning

**Question**: What happens when an action's interface changes?

**Answer**: **Strict Versioning**

- Actions have explicit versions (e.g., `CreateCalendarEvent@v1`)
- New versions registered alongside old ones
- Old versions deprecated but never removed
- Chain definitions reference specific versions

**Rationale**: Breaking changes to actions would silently break user chains. Strict versioning prevents this.

### 15. Observability

**Question**: How do we trace chain-created entities back to their chain?

**Answer**: **Correlation ID Propagation**

- Each chain execution gets a unique correlation ID
- Correlation ID flows through all middleware and action handlers
- `chain_entity_mappings` table tracks which entities were created by which chain
- Enables "this calendar event was created by the Doctor Appointment chain"

**Rationale**: Families need to understand why things appeared. Correlation IDs make the chain engine transparent, not magical.

### 16. Domain Model

**Question**: What are the core aggregates?

**Answer**: **Two Aggregates**

1. **`ChainDefinition`** — Template/blueprint for a chain
   - Contains: trigger, steps (with conditions), metadata
   - Owned by a family
   - Can be enabled/disabled

2. **`ChainExecution`** — Runtime saga instance
   - Created when trigger fires for a matching ChainDefinition
   - Contains: current state, step results, context
   - Immutable history (completed executions never modified)

**Rationale**: Separation of template vs runtime is standard saga pattern. ChainDefinition is the "what", ChainExecution is the "when/how it went".

### 17. V1 Templates

Three pre-built chain templates ship with V1:

1. **Doctor Appointment** (flagship)
   - Trigger: `HealthAppointmentScheduledEvent`
   - Steps: Create calendar event → Create preparation task → Schedule 3 reminders
   - Saves: ~10 min, reduces 4 mental items

2. **Task Assignment** (simple)
   - Trigger: `TaskAssignedEvent`
   - Steps: Send notification → Create calendar event (if deadline exists)
   - Saves: ~2 min per assignment

3. **Calendar Reminder** (universal)
   - Trigger: `CalendarEventCreatedEvent`
   - Steps: Schedule configurable reminders (24h, 2h, 15min before)
   - Saves: Never miss an event

### 18. UI Placement

**Answer**: Top-level "Automations" navigation item

- Not buried in settings
- Equal prominence to Calendar, Tasks, etc.
- Reflects that automations are a first-class feature

---

## Technical Constraints

1. **Must coexist with Wolverine**: Dual dispatch, no breaking changes to existing handlers
2. **PostgreSQL only**: No additional infrastructure (no Redis, no RabbitMQ for V1)
3. **Single process**: V1 executes in-process, no distributed saga
4. **EF Core migrations**: Follow existing migration pattern in `src/FamilyHub.Api/Migrations/`
5. **Hot Chocolate GraphQL**: Follow Input→Command pattern (ADR-003)
6. **Wolverine handlers**: Static class + static Handle method (existing convention)
7. **RLS on all tables**: Family-scoped data isolation

---

## Success Indicators

### Functional

- [ ] Chain definitions can be created, updated, enabled, disabled, deleted
- [ ] Chain executions trigger automatically when matching events fire
- [ ] Steps execute sequentially with context flowing between them
- [ ] Failed steps trigger compensation for completed steps
- [ ] Circuit breaker prevents cascading failures
- [ ] Scheduler picks up delayed steps and executes them
- [ ] V1 templates work end-to-end

### Quality

- [ ] >95% unit test coverage on state machine transitions
- [ ] Integration tests for full chain execution
- [ ] RLS enforced on all 6 tables
- [ ] Correlation IDs traceable across all chain-created entities
- [ ] <100ms overhead per step (middleware pipeline)

### User Experience

- [ ] Chain builder validates type compatibility in real-time
- [ ] Execution history shows clear success/failure/partial states
- [ ] Permanent failures notify user with actionable context
- [ ] Templates are one-click to install

---

## Risks & Mitigations

### Risk: Circular chain triggers

**Mitigation**: Chain engine checks for cycles at definition time. A chain cannot trigger itself (directly or transitively).

### Risk: Performance impact on existing event handlers

**Mitigation**: Dual dispatch is async. Chain engine subscribes independently and doesn't block Wolverine handlers.

### Risk: Schema migration complexity

**Mitigation**: Dedicated `event_chain` schema isolates all tables. No modifications to existing schemas.

### Risk: User creates chain that generates excessive notifications

**Mitigation**: Rate limiting per chain execution (max 10 actions per chain). Configurable per family.

### Risk: Stale scheduler jobs

**Mitigation**: `picked_up_at` column with TTL. Jobs not completed within TTL are released for re-pickup.

---

## Notes from Exploration

### Current Event Infrastructure (Working)

- `IDomainEvent` + `DomainEvent` base record
- `AggregateRoot.RaiseDomainEvent()` pattern
- `AppDbContext.SaveChangesAsync()` publishes via `IMessageBus`
- Wolverine discovers handlers by convention
- 4 domain events exist: `UserRegisteredEvent`, `UserFamilyAssignedEvent`, `UserFamilyRemovedEvent`, `FamilyCreatedEvent`
- 3 event handlers exist (all logging-only placeholders)

### What Chain Engine Adds

- Orchestration layer on top of existing events
- Saga state management (persist + resume)
- Compensation for partial failures
- User-configurable chains (not just hard-coded handlers)
- Cross-domain workflow coordination
- Execution monitoring + history
