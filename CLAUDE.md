# Family Hub - Claude Code Guide

## CRITICAL CONTEXT

**Family Hub** is a privacy-first, cloud-based family organization platform focused on intelligent **event chain automation** (automatic cross-domain workflows that no competitor offers).

**Tech Stack:** .NET Core 10 (C# 14, Hot Chocolate GraphQL) | Angular v21 (TypeScript, Tailwind) | PostgreSQL 16 (RLS) | RabbitMQ | Zitadel OAuth | Docker Compose→Kubernetes | Vogen 8.0+

**Phase:** Phase 0 - Foundation & Tooling (IN PROGRESS). OAuth integration completed. Next: Frontend OAuth, then Phase 1 Core MVP.

**Architecture:** Modular Monolith (Phase 1-4) → Microservices (Phase 5+). See [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md).

**Development:** Single developer + Claude Code AI (60-80% boilerplate). DDD with 8 modules. Event-driven (RabbitMQ). 10-14 months to MVP.

**Strategic Pivot:** Cloud-based SaaS FIRST. Self-hosting deferred to Phase 7+ (post-MVP). Event chains are PRIMARY differentiator.

---

## QUICK START

### Implementing Features

1. Check [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md) - phase scheduling
2. Find in [FEATURE_BACKLOG.md](docs/product-strategy/FEATURE_BACKLOG.md) - priority, RICE score
3. Review [wireframes.md](docs/ux-design/wireframes.md) - UI layout
4. Identify module in [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md)
5. Check [event-chains-reference.md](docs/architecture/event-chains-reference.md) - workflow impact
6. Implement following [IMPLEMENTATION_WORKFLOW.md](docs/development/IMPLEMENTATION_WORKFLOW.md)

### Architecture Questions

Read: [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md), [ADR-002](docs/architecture/ADR-002-OAUTH-WITH-ZITADEL.md), [ADR-003](docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md), [ADR-004](docs/architecture/ADR-004-PLAYWRIGHT-MIGRATION.md), [ADR-006](docs/architecture/ADR-006-EMAIL-ONLY-AUTHENTICATION.md), [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md)

### Planning & Roadmap

Read: [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md), [FEATURE_BACKLOG.md](docs/product-strategy/FEATURE_BACKLOG.md), [risk-register.md](docs/product-strategy/risk-register.md)

---

## DOCUMENTATION

**51 documents** in 9 folders. Complete map: [docs/INDEX.md](docs/INDEX.md)

**Development Patterns:**

- [WORKFLOWS.md](docs/development/WORKFLOWS.md) - EF Core, Vogen, Testing, GraphQL, Playwright
- [PATTERNS.md](docs/development/PATTERNS.md) - DDD, Value Objects, Aggregates, Events
- [IMPLEMENTATION_WORKFLOW.md](docs/development/IMPLEMENTATION_WORKFLOW.md) - Standard feature workflow

---

## 8 DDD MODULES

**Modules:** Auth, Calendar, Task, Shopping, Health, Meal Planning, Finance, Communication

Each owns: Domain entities/aggregates, domain events (RabbitMQ), GraphQL schema types, PostgreSQL schema (RLS).

**Full spec:** [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md)

---

## EVENT CHAIN AUTOMATION

**Flagship feature:** Automatic cross-domain workflows. Example: Doctor appointment → calendar event → preparation task → prescription → shopping list → pickup task → refill reminder.

**Impact:** Saves 10-30 minutes per workflow, reduces cognitive load 40-60%.

**Implementation:** Event-driven (RabbitMQ), in-process (Phase 1-4), network-based (Phase 5+), Saga pattern.

**Full specs:** [event-chains-reference.md](docs/architecture/event-chains-reference.md)

---

## DEVELOPMENT WORKFLOWS

**CRITICAL: Always follow [IMPLEMENTATION_WORKFLOW.md](docs/development/IMPLEMENTATION_WORKFLOW.md) for feature implementation.**

### Database Migrations

Use EF Core Code-First. One DbContext per module. See [WORKFLOWS.md#database-migrations](docs/development/WORKFLOWS.md#database-migrations-with-ef-core)

### Value Objects

Use Vogen for ALL value objects (UserId, Email, FamilyName, etc.). See [WORKFLOWS.md#value-objects](docs/development/WORKFLOWS.md#value-objects-with-vogen) and [PATTERNS.md#value-objects](docs/development/PATTERNS.md#value-object-patterns)

### Testing

- FluentAssertions for ALL assertions (never xUnit Assert)
- [Theory, AutoNSubstituteData] for tests with dependencies
- Create Vogen value objects manually in tests
- See [WORKFLOWS.md#testing](docs/development/WORKFLOWS.md#testing-patterns)

### GraphQL

Separate Input DTOs (primitives) → MediatR Commands (Vogen value objects). See [WORKFLOWS.md#graphql](docs/development/WORKFLOWS.md#graphql-inputcommand-pattern) and [ADR-003](docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)

### E2E Tests

Use Playwright (migrated from Cypress January 2026). API-first testing for event chains. Zero-retry policy. See [WORKFLOWS.md#e2e](docs/development/WORKFLOWS.md#e2e-testing-with-playwright) and [ADR-004](docs/architecture/ADR-004-PLAYWRIGHT-MIGRATION.md)

### DDD Patterns

Aggregates, Domain Events, Repositories, CQRS. See [PATTERNS.md](docs/development/PATTERNS.md)

### Automatic Formatting

PostToolUse hooks run Prettier+ESLint (TypeScript) and dotnet format (C#) automatically. See [WORKFLOWS.md#automatic-code-formatting](docs/development/WORKFLOWS.md#automatic-code-formatting) and [HOOKS.md](docs/development/HOOKS.md)

---

## CONTRIBUTING

**Issue Templates:** `.github/ISSUE_TEMPLATE/` - Feature Request, Bug Report, Phase Deliverable, Research, Technical Debt

**PR Template:** `.github/PULL_REQUEST_TEMPLATE.md` - Architecture impact, testing, quality checklists

**Labels:** 60+ labels across Type, Phase, Service, Status, Priority, Domain, Effort, Special categories

**Guide:** [CONTRIBUTING.md](CONTRIBUTING.md)

---

## STRATEGIC CONSTRAINTS

### What TO Do

1. Launch as cloud-based SaaS (NOT self-hosted initially)
2. Prioritize event chain automation (core differentiator)
3. Follow phases sequentially (no skipping)
4. Use Claude Code for 60-80% of boilerplate
5. Respect DDD module boundaries

### What NOT to Do

- Don't implement Federation Service (Phase 7+, post-MVP)
- Don't skip phases in roadmap
- Don't ignore event chains when designing features
- Don't assume features - check FEATURE_BACKLOG.md
- Don't break DDD boundaries
- Don't duplicate documentation

---

## CLAUDE CODE GUIDE

### Implementation Workflow

**Standard process (MANDATORY):**

1. User requests feature (moderate detail)
2. Ask clarifying questions (AskUserQuestion)
3. Spawn feature-dev:code-explorer (find existing patterns)
4. Spawn feature-dev:code-architect (design following patterns)
5. Implement following EXACT patterns from subagents
6. Generate tests following existing test patterns

**Goal:** 80-90% code correctness (vs 40-60% baseline)

**See:** [IMPLEMENTATION_WORKFLOW.md](docs/development/IMPLEMENTATION_WORKFLOW.md)

### Subagent Decision Tree

- Simple (1-2 files): Direct implementation
- Moderate (3-5 files): Explore → Implement
- Complex (5+ files): Explore → Plan → Implement
- Architectural: Explore → Plan → Review → Implement

### Preferred Subagents

- **feature-dev:code-explorer** - Pattern discovery in codebase
- **feature-dev:code-architect** - Implementation design
- **Explore** - Codebase navigation
- **Plan** - Architecture planning
- **Specialized domain agents** - frontend-developer, backend-developer, typescript-pro, angular-architect

### Tool Preferences

**Use MORE:**

- Serena (find_symbol, find_referencing_symbols, replace_symbol_body)
- Context7 (up-to-date library docs)
- Sequential-thinking (complex decisions)
- Task subagents (exploration, planning)

**Use LESS:**

- Extensive code comments (code should be self-documenting)

### Commit Format

```
<type>(<scope>): <summary> (#<issue>)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
```

Types: feat, fix, docs, style, refactor, test, chore

### Educational Insights

**ALWAYS provide "Insight" boxes explaining:**

- Architectural patterns discovered
- DDD concepts and trade-offs
- Project-specific patterns
- Design decisions

---

**Last updated:** 2026-01-06
**Version:** 4.0.0 (Optimized for token efficiency and code quality)
