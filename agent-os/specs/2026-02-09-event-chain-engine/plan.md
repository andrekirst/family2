# Event Chain Engine — Implementation Plan

**Created**: 2026-02-09
**Spec Folder**: `agent-os/specs/2026-02-09-event-chain-engine/`

---

## Context

The Event Chain Engine is Family Hub's **primary competitive differentiator** — automated cross-domain workflows that save families 10-30 minutes per workflow and reduce mental load by 3-5 fewer things to remember. No competitor (Cozi, FamilyWall, TimeTree, Picniic) offers this capability.

The existing codebase has a strong foundation: `IDomainEvent`, `AggregateRoot.RaiseDomainEvent()`, Wolverine in-process bus, and CQRS abstraction (`ICommandBus`/`IQueryBus`). The Event Chain Engine builds an **orchestration layer on top** — turning individual domain events into coordinated multi-step workflows with saga state management, compensation, and user-defined chain creation.

This plan covers **Task 1 only: saving the spec documentation** and **creating the GitHub epic issue**. The three implementation epics (Core Engine, GraphQL API, Frontend UI) will be tracked as separate future work.

---

## Decisions Made (Interview Summary)

| Topic | Decision |
|-------|----------|
| **Execution model** | Saga/Orchestrator pattern — each chain is a state machine with forward + compensation steps |
| **Failure semantics** | Circuit breaker + partial completion — skip unhealthy steps, retry when circuit closes |
| **User chains (V1)** | Full custom chains — users build chains from a trigger/action catalog |
| **State store** | PostgreSQL table — `chain_executions` + `step_executions` for saga state |
| **Compensation** | Semantic undo — non-compensatable steps send correction notifications instead of true rollback |
| **Module coupling** | Plugin/registry pattern — modules register triggers/actions with `IChainRegistry` at startup |
| **Scheduling** | PostgreSQL polling with `SELECT FOR UPDATE SKIP LOCKED` for concurrent safety |
| **GitHub issues** | Single epic issue with task checklist (3 separate epics: engine + API + UI) |
| **Chain validation** | Type-compatible validation — output/input type matching prevents nonsensical chains |
| **Privacy model** | Role-based (RLS) — parents see all, members see own, children see simplified |
| **Partial completion UX** | Background + escalation — silent retries, only notify on permanent failure |
| **Action versioning** | Strict versioning — actions versioned, deprecated but never removed |
| **Context flow** | Typed step outputs namespaced by step **alias** (not position) for stability |
| **Step execution** | Custom middleware pipeline — Logging → Metrics → CircuitBreaker → Retry → Compensation → ActionHandler |
| **GraphQL API** | Full CRUD + subscriptions — approved schema (see `graphql-api.md`) |
| **Coexistence** | Dual dispatch — both Wolverine handlers and chain engine process events independently |
| **Conditions** | Simple if/else per step — condition evaluates against context, skip if false |
| **Observability** | Correlation ID propagation — every chain-created entity traceable via correlation ID |
| **Scheduler safety** | `SELECT FOR UPDATE SKIP LOCKED` — PostgreSQL row locking for concurrent job execution |
| **Testing priority** | Unit tests first — state machine transitions, validation, conditions, context flow |
| **Entity awareness** | Separate mapping table — `chain_entity_mappings` in event_chain schema |
| **Domain model** | Two aggregates: `ChainDefinition` (template) + `ChainExecution` (runtime saga) |
| **V1 templates** | Doctor Appointment (flagship), Task Assignment (simple), Calendar Reminder (universal) |
| **Epic scope** | Split into 3 epics: (1) Core engine, (2) GraphQL API, (3) Frontend UI |
| **DB schema** | Dedicated `event_chain` PostgreSQL schema |
| **UI placement** | Top-level "Automations" nav item |
| **Labels** | Auto-selected based on content |
| **Pipeline config** | Fixed global pipeline — all chains share the same middleware stack |

---

## Task 1: Save Spec Documentation

Create `agent-os/specs/2026-02-09-event-chain-engine/` with the following files:

### Files to Create

1. **`plan.md`** — This full plan (copy of this document)
2. **`shape.md`** — Shaping notes capturing all interview decisions, scope, and context
3. **`standards.md`** — Relevant standards: `backend/domain-events`, `architecture/event-chains`, `architecture/ddd-modules`, `database/ef-core-migrations`, `database/rls-policies`, `backend/graphql-input-command`, `backend/vogen-value-objects`, `testing/unit-testing`
4. **`references.md`** — Pointers to existing event infrastructure code and patterns
5. **`graphql-api.md`** — Complete approved GraphQL API schema (types, queries, mutations, subscriptions)
6. **`database-schema.md`** — Complete approved PostgreSQL schema (6 tables + RLS + indexes)

### Existing Files/Patterns to Reuse

- `src/FamilyHub.Api/Common/Domain/IDomainEvent.cs` — Event interface
- `src/FamilyHub.Api/Common/Domain/DomainEvent.cs` — Base event record
- `src/FamilyHub.Api/Common/Domain/AggregateRoot.cs` — Aggregate base with RaiseDomainEvent()
- `src/FamilyHub.Api/Common/Infrastructure/Messaging/WolverineCommandBus.cs` — CQRS bus
- `src/FamilyHub.Api/Common/Database/AppDbContext.cs` — Event extraction on SaveChanges
- `src/FamilyHub.Api/Features/Auth/Domain/Events/` — Example domain events
- `src/FamilyHub.Api/Features/Auth/Application/EventHandlers/` — Example Wolverine handlers

---

## Task 2: Create GitHub Epic Issue

Create a single epic GitHub issue titled **"Event Chain Engine — Saga Orchestrator with Custom Chain Builder"** with:

### Labels (auto-selected)

- `type-feature` — New feature
- `priority-p1` — Core differentiator
- `effort-xl` — Multiple weeks of work
- `domain-event-chains` — Event chain automation
- `phase-2` — Health Integration & Event Chains
- `ai-assisted` — Claude Code will help implement

### Issue Structure

The issue body will contain:

1. **Overview** — What the Event Chain Engine is and why it matters
2. **Architecture Decisions** — Summary table of all decisions from the interview
3. **Technical Design** — Key components (saga orchestrator, plugin registry, middleware pipeline, scheduler, compensation)
4. **Database Schema** — Approved 6-table schema
5. **GraphQL API** — Approved full API surface
6. **V1 Templates** — Doctor Appointment, Task Assignment, Calendar Reminder
7. **Task Checklist** — Broken into 3 epic sections:
   - Epic 1: Core Engine Module (~15 tasks)
   - Epic 2: GraphQL API + Integration (~10 tasks)
   - Epic 3: Frontend Chain Builder UI (~10 tasks)

---

## Verification

After execution:

1. **Spec folder exists** at `agent-os/specs/2026-02-09-event-chain-engine/` with all 6 files
2. **GitHub issue created** and visible at the returned URL
3. **Labels applied** correctly to the issue
4. **Issue body** contains the complete technical design with task checklists
5. **Spec files** are internally consistent (GraphQL API matches DB schema, decisions match shape notes)
