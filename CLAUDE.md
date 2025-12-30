# Family Hub - Claude Code Guide

## 🚨 CRITICAL CONTEXT - Read First

**Family Hub** is a privacy-first, cloud-based family organization platform that reduces mental load through intelligent **event chain automation**.

**Primary Differentiator:** Event Chain Automation - Automatic cross-domain workflows that no competitor offers:
- Doctor appointment → calendar event → preparation task → prescription → shopping list → pickup task → refill reminder
- Meal plan → shopping list → budget tracking → recipe suggestions
- Task assignment → notifications → reminders → completion tracking

**⚠️ CRITICAL ARCHITECTURE DECISION:** Starting with Modular Monolith, not microservices from day one. See [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) for rationale.

**Technology Stack:**

| Layer | Technology | Notes |
|-------|------------|-------|
| Backend | .NET Core 10 / C# 14 | Hot Chocolate GraphQL |
| Frontend | Angular v21 + TypeScript | Tailwind CSS |
| Database | PostgreSQL 16 | Row-Level Security (RLS) |
| Event Bus | RabbitMQ | In-process Phase 1-4, network Phase 5+ |
| Auth | Zitadel | External OAuth 2.0 / OIDC |
| Infrastructure | Docker Compose → Kubernetes | Phase 1-4 → Phase 5+ |
| Monitoring | Prometheus + Grafana + Seq | |
| Value Objects | Vogen 8.0+ | Source generator for strongly-typed primitives |

**📍 CURRENT PHASE: Phase 0 - Foundation & Tooling (IN PROGRESS)**
- **Phase 1 Preparation:** ✅ COMPLETED (December 2024 - Issues #4-11)
- **OAuth Integration:** ✅ COMPLETED (December 2024 - Zitadel OAuth 2.0, 7 days)
- **51 core documents** (280,000+ words) organized in 9 thematic folders
- **Technology stack confirmed:** .NET Core 10, Angular v21, GraphQL, RabbitMQ, Zitadel
- **Architecture validated:** Modular monolith first (ADR-001), OAuth with Zitadel (ADR-002)
- **Next:** Frontend OAuth integration, then Phase 1 Core MVP

**Strategic Pivot (December 2024):**
- ✅ Launch as cloud-based online service FIRST
- ⚠️ Self-hosting and federation DEFERRED to Phase 7+ (post-MVP)
- 🎯 Focus on event chain automation as PRIMARY differentiator
- ⏱️ Faster time to market: 12 months vs 15-18 months

**Development Approach:**
- **Single developer** + **Claude Code AI** (60-80% of boilerplate)
- Domain-Driven Design (DDD) with **8 bounded contexts (modules)**
- **Modular Monolith** (Phase 1-4) → Microservices (Phase 5+)
- Event-driven architecture with RabbitMQ
- Incremental delivery: **10-14 months** to MVP (reduced from 16-22)

---

## 💡 QUICK START BY TASK

### Implementing Features

1. **Check phase:** [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md) - Is this feature scheduled for current phase?
2. **Find feature:** [FEATURE_BACKLOG.md](docs/product-strategy/FEATURE_BACKLOG.md) - Priority, dependencies, RICE score
3. **Review wireframes:** [wireframes.md](docs/ux-design/wireframes.md) - UI layout and user flow
4. **Check design system:** [design-system.md](docs/ux-design/design-system.md) - Components and styling
5. **Identify module:** [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md) - Which of 8 modules?
6. **Review events:** Does this feature publish/consume domain events?
7. **Check event chains:** [event-chains-reference.md](docs/architecture/event-chains-reference.md) - Part of workflow?
8. **Verify accessibility:** [accessibility-strategy.md](docs/ux-design/accessibility-strategy.md) - WCAG 2.1 AA requirements
9. **Review risks:** [risk-register.md](docs/product-strategy/risk-register.md) - Mitigation strategies
10. **Implement:** Follow module patterns, test thoroughly

### Architecture Questions

**Read these 4 docs:**
1. [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) - Modular monolith decision rationale
2. [ADR-002](docs/architecture/ADR-002-OAUTH-WITH-ZITADEL.md) - OAuth 2.0 with Zitadel (vs Auth0, Keycloak, ASP.NET Identity)
3. [Domain Model](docs/architecture/domain-model-microservices-map.md) - 8 DDD modules, domain events, GraphQL schemas
4. [Event Chains Reference](docs/architecture/event-chains-reference.md) - Event-driven patterns and workflows

### Planning & Roadmap Questions

**Read these 3 docs:**
1. [Implementation Roadmap](docs/product-strategy/implementation-roadmap.md) - 6-phase plan (Phase 0-6), deliverables, timeline
2. [Feature Backlog](docs/product-strategy/FEATURE_BACKLOG.md) - 208 features prioritized by RICE score
3. [Risk Register](docs/product-strategy/risk-register.md) - 35 risks with mitigation strategies

### Product Strategy Questions

**Read these 3 docs:**
1. [Product Strategy](docs/product-strategy/PRODUCT_STRATEGY.md) - Vision, personas, strategic pillars, positioning
2. [UX Research Report](docs/ux-design/ux-research-report.md) - 6 personas, user journeys, competitive analysis
3. [Executive Summary](docs/executive-summary.md) - 15-minute overview of vision, market, strategy

---

## 📚 DOCUMENTATION

**51 documents** organized in 9 thematic folders. See **[docs/INDEX.md](docs/INDEX.md)** for complete documentation map.

**Quick Access:**
- **Architecture:** [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md), [ADR-002](docs/architecture/ADR-002-OAUTH-WITH-ZITADEL.md), [ADR-003](docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)
- **Development:** [Domain Model](docs/architecture/domain-model-microservices-map.md), [Event Chains](docs/architecture/event-chains-reference.md)
- **Product:** [Product Strategy](docs/product-strategy/PRODUCT_STRATEGY.md), [Roadmap](docs/product-strategy/implementation-roadmap.md), [Feature Backlog](docs/product-strategy/FEATURE_BACKLOG.md)

---

## 🏗️ 8 DDD MODULES

**Architecture:** Modular Monolith (Phase 1-4) → Microservices (Phase 5+). See [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md).

**Modules:** Auth, Calendar, Task, Shopping, Health, Meal Planning, Finance, Communication

Each module owns: Domain entities/aggregates, domain events (RabbitMQ), GraphQL schema types, PostgreSQL schema (RLS).

**Full specification:** [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md) - domain entities, events, GraphQL schemas.

---

## ⚡ EVENT CHAIN AUTOMATION (Flagship Feature)

**What it is:** Automatic cross-domain workflows (no competitor offers this). Example: Doctor appointment → calendar → preparation task → prescription → shopping list → pickup task → refill reminder.

**Why it matters:** Saves 10-30 minutes per workflow, eliminates 3-5 things to remember, reduces cognitive load (40-60% stress reduction).

**Implementation:** Event-driven (RabbitMQ), in-process (Phase 1-4), network-based (Phase 5+), Saga pattern for complex workflows.

**Full specifications:** [event-chains-reference.md](docs/architecture/event-chains-reference.md) - 10 event chains, patterns, monitoring, testing.

---

## 🛠️ DEVELOPMENT WORKFLOWS

### Database Migrations with EF Core

**CRITICAL:** Use EF Core Code-First migrations for ALL schema changes (never custom SQL scripts).

**Pattern:** One DbContext per module (Auth, Calendar, etc.), each targeting its own PostgreSQL schema. Fluent API configurations in `IEntityTypeConfiguration<T>` classes, PostgreSQL-specific features (RLS, triggers) via `migrationBuilder.Sql()`.

**Create migration:** `dotnet ef migrations add <Name> --context AuthDbContext --project Modules/FamilyHub.Modules.Auth --startup-project FamilyHub.Api`

**Apply migration:** `dotnet ef database update --context AuthDbContext` (dev) or `await context.Database.MigrateAsync()` (prod in Program.cs)

**Vogen Integration:** Use `HasConversion(new UserId.EfCoreValueConverter())` in entity configurations.

**Reference:** Original SQL design scripts in `/database/docs/reference/sql-design/` (informational only, NOT executed).

### Value Objects with Vogen

**CRITICAL:** Use Vogen for ALL value objects (never manual base classes). Vogen source generator auto-generates equality, validation, EF Core converters, JSON serialization.

**Pattern:** `[ValueObject<T>(conversions: Conversions.EfCoreValueConverter)]` on `readonly partial struct`. Implement `Validate(T value)` for domain validation, `NormalizeInput(T input)` for normalization.

**Creation:** `UserId.New()` (new GUID), `Email.From("user@example.com")` (with validation). Use `TryFrom()` for safe creation.

**EF Core:** `HasConversion(new UserId.EfCoreValueConverter())` in `IEntityTypeConfiguration`.

**Examples:** See `/src/api/FamilyHub.SharedKernel/Domain/ValueObjects/` for Email, UserId, FamilyId patterns.

### Test Assertions with FluentAssertions

**CRITICAL:** Use FluentAssertions for ALL assertions (never xUnit `Assert.*`). Better readability, error messages, and async support.

**Pattern:** `actual.Should().Be(expected)`, `result.Should().NotBeNull()`, `collection.Should().HaveCount(3)`, `await act.Should().ThrowAsync<T>()`.

**Async:** Use `ThrowAsync<T>()` for async code (not `Throw<T>()`). Use `CompleteWithinAsync()` for timeouts.

**Examples:** See test files in `/src/api/tests/` for patterns. Docs: https://fluentassertions.com/

### Test Data Generation with AutoFixture

**CRITICAL:** Use `[Theory, AutoNSubstituteData]` for ALL tests with dependencies (never manual mocks in constructors). Auto-injects NSubstitute mocks via method parameters.

**Pattern:** `public async Task Test(IUserRepository repo, IMediator mediator) { ... }` - dependencies auto-injected, configure only what matters for the test.

**Vogen Policy:** Always create Vogen value objects manually: `UserId.New()`, `Email.From()`. Do NOT auto-generate (improves test clarity).

**When to use:** Command/query handler tests, integration tests. When NOT to use: Domain entity tests (use explicit test data with `[Fact]`).

**Custom attribute:** `AutoNSubstituteDataAttribute` in `/src/api/tests/FamilyHub.Tests.Unit/`. Performance: ~15-20ms overhead per test (acceptable up to 500+ tests).

### GraphQL Inputs and Commands

**Pattern:** Maintain separate GraphQL Input DTOs (primitive types) that map to MediatR Commands (Vogen value objects) in mutation methods.

**Why:** HotChocolate cannot natively deserialize Vogen value objects from JSON. Input → Command mapping provides explicit conversion point and framework compatibility.

**Example:** `CreateFamilyInput { string Name }` → `CreateFamilyCommand { FamilyName Name }` via `new CreateFamilyCommand(FamilyName.From(input.Name))` in mutation.

**Decision rationale:** [ADR-003](docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md) - attempted command-as-input pattern, failed due to Vogen incompatibility.

### Contributing & Creating Issues

**Issue Templates** (`.github/ISSUE_TEMPLATE/`):
- Feature Request (RICE scoring, user stories)
- Bug Report (severity, reproduction steps)
- Phase Deliverable (epic coordination)
- Research & Documentation
- Technical Debt
- Blank (custom)

**Pull Requests:** Use `.github/PULL_REQUEST_TEMPLATE.md` with architecture impact, testing, quality checklists.

**Labels System (60+ labels):**

| Category | Labels |
|----------|--------|
| Type | feature, bug, epic, research, docs, tech-debt, infrastructure, security, performance |
| Phase | phase-0 through phase-6, phase-7-future |
| Service | auth, calendar, task, shopping, health, meal, finance, communication, frontend, infrastructure, multiple |
| Status | triage, planning, ready, in-progress, blocked, review, testing, done, wontfix |
| Priority | p0 (critical), p1 (high), p2 (medium), p3 (low) |
| Domain | auth, calendar, tasks, shopping, health, meals, finance, notifications, event-chains, mobile, ux |
| Effort | xs (<1 day), s (1-3 days), m (1 week), l (2 weeks), xl (>2 weeks) |
| Special | good-first-issue, help-wanted, breaking-change, needs-documentation, needs-design, ai-assisted |

**Create labels:** `./scripts/create-labels.sh`

**Full guide:** [CONTRIBUTING.md](CONTRIBUTING.md)

**Why this matters:**
- Structured issues aligned with DDD architecture
- RICE scoring maintains prioritization methodology
- Phase alignment ensures roadmap adherence
- Event chain awareness prompts cross-module impact consideration
- AI-friendly templates (Claude Code generates 60-80% of code)

---

## ⚠️ STRATEGIC CONTEXT & CONSTRAINTS

### Strategic Decisions

1. **Online Service First** - Launch as cloud-based SaaS, NOT self-hosted initially. Self-hosting/federation deferred to Phase 7+ (post-MVP). Focus on event chain automation as PRIMARY differentiator.

2. **Event Chains are #1 Priority** - This is what makes Family Hub unique. No competitor offers automated cross-domain workflows. Must work flawlessly. Saves users 10-30 minutes per workflow.

3. **Single Developer + AI** - Project designed for AI-assisted solo development. Claude Code generates 60-80% of boilerplate, tests, schemas. Quality over speed. Incremental delivery.

4. **Privacy-First but Pragmatic** - GDPR compliant, no data selling, transparent security. Cloud-hosted for ease of use initially. Self-hosting for privacy advocates comes later (Phase 7+).

### What NOT to Do

❌ **Don't implement Federation Service** (Phase 7+, deferred - not in scope until post-MVP)
❌ **Don't skip phases** in implementation roadmap (follow sequential delivery)
❌ **Don't ignore event chains** when designing features (core innovation)
❌ **Don't assume features** - check `FEATURE_BACKLOG.md` for priorities (208 features RICE-scored)
❌ **Don't break DDD boundaries** - respect module ownership (8 modules, clear responsibilities)
❌ **Don't duplicate documentation** - reference `/docs/` instead of repeating content

### Tips for Claude Code

**When implementing features:**
1. Check [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md) for phase scheduling
2. Review [FEATURE_BACKLOG.md](docs/product-strategy/FEATURE_BACKLOG.md) for priority and RICE score
3. Consult [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md) for module ownership

**When asked about architecture:**
1. Start with [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md) for strategic decision
2. Review [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md) for DDD patterns
3. Check [event-chains-reference.md](docs/architecture/event-chains-reference.md) for event-driven patterns

**When planning work:**
- Follow phases in [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md) - don't skip ahead
- Each phase has clear deliverables and success criteria
- Validate assumptions before implementing
- Keep Federation deferred - online service first

---

_This guide was created to help Claude Code navigate the Family Hub project efficiently. For full context, always refer to the `/docs/` folder._

**Last updated:** 2025-12-30
**CLAUDE.md version:** 3.0 (Performance optimized: 1,083 → 266 lines, 75% reduction)
