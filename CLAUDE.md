# Family Hub - Claude Code Guide

## Critical Context

**Family Hub** is a privacy-first family organization platform focused on intelligent **event chain automation** (automatic cross-domain workflows that no competitor offers).

**Current State:** Active development with working Auth and Family modules. Backend API, frontend SPA, and test suite all operational.

**Tech Stack:** .NET 9 (C#, Hot Chocolate GraphQL, Wolverine, Vogen) | Angular 19 (TypeScript, Tailwind, Apollo) | PostgreSQL | Keycloak (OAuth 2.0 / OIDC) | Docker

**Phase:** Phase 1 - MVP. Family creation and invitation system implemented.

**Architecture Strategy:** Modular Monolith (feature-folder layout). See [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md).

**Message Bus:** Wolverine (NOT MediatR). Handlers are static classes with `Handle()` methods, auto-discovered via parameter injection.

---

## Current Implementation Status

### Auth Module (`src/FamilyHub.Api/Features/Auth/`)

- User registration + login via Keycloak OIDC
- `GetCurrentUser` query with role-based permissions
- Domain events: `UserRegisteredEvent`, `UserFamilyAssignedEvent`, `UserFamilyRemovedEvent`
- `IUserService` + `ClaimNames` for JWT claim extraction

### Family Module (`src/FamilyHub.Api/Features/Family/`)

- **Aggregates:** `Family`, `FamilyInvitation`
- **Entities:** `FamilyMember`
- **Value Objects:** `FamilyId`, `FamilyName`, `FamilyRole`, `FamilyMemberId`, `InvitationId`, `InvitationToken`, `InvitationStatus`
- **Commands:** CreateFamily, SendInvitation, AcceptInvitation, AcceptInvitationById, DeclineInvitation, DeclineInvitationById, RevokeInvitation
- **Queries:** GetMyFamily, GetFamilyMembers
- **Permission system:** `FamilyRole.GetPermissions()` returns `string[]` (Owner > Admin > Member hierarchy)
- **Authorization:** `FamilyAuthorizationService` for backend enforcement
- **Subfolder layout:** `Commands/{Name}/Command.cs, Handler.cs, MutationType.cs, Validator.cs`

### Frontend (`src/frontend/family-hub-web/`)

- Angular 19 with signals-based architecture
- `FamilyPermissionService` (computed signals) in `core/permissions/`
- Components: create-family-dialog, family-settings, invite-member, members-list, pending-invitations, invitation-accept
- Apollo GraphQL client with typed operations

### Tests (`tests/FamilyHub.UnitTests/`)

- 81 tests passing (xUnit + FluentAssertions)
- Fake repository pattern (inner classes implementing interfaces, in-memory state)
- Domain tests: FamilyAggregate, FamilyInvitation, FamilyMember, FamilyRole
- Handler tests: CreateFamily, SendInvitation, AcceptInvitation, AcceptInvitationById, GetCurrentUser

### Known Gaps

- `RegisterUser` mutation does not return permissions (only `GetCurrentUser` does)
- No NSubstitute yet (planned migration from fake repos)
- Permission caching not implemented (refetch after state change is current pattern)

---

## Domain-Specific Guides

**Choose the right guide for your task:**

### Backend Development

→ **[docs/guides/BACKEND_DEVELOPMENT.md](docs/guides/BACKEND_DEVELOPMENT.md)** - .NET, C#, GraphQL, DDD patterns

- When: Implementing backend features, domain logic, GraphQL APIs
- Covers: Wolverine handlers, Vogen VOs, GraphQL Input→Command, Domain events, Testing patterns
- Sub-guides in `docs/guides/backend/` for handler patterns, authorization, testing, GraphQL, EF Core, Vogen, domain events

### Frontend Development

→ **[docs/guides/FRONTEND_DEVELOPMENT.md](docs/guides/FRONTEND_DEVELOPMENT.md)** - Angular, TypeScript, component architecture

- When: Planning frontend architecture, components, pages, E2E tests
- Covers: Component architecture, Apollo GraphQL, Playwright, OAuth PKCE

### Database Work

→ **[docs/guides/DATABASE_DEVELOPMENT.md](docs/guides/DATABASE_DEVELOPMENT.md)** - PostgreSQL, EF Core migrations, RLS

- When: Planning database schema, migrations, RLS policies
- Covers: Migration workflows, schema design, RLS patterns

### Infrastructure & DevOps

→ **[docs/guides/INFRASTRUCTURE_DEVELOPMENT.md](docs/guides/INFRASTRUCTURE_DEVELOPMENT.md)** - Docker, K8s, CI/CD

- When: Planning infrastructure, deployment, observability
- Covers: Docker setup, K8s deployment patterns, CI/CD pipelines

### GitHub Workflow

→ **[.github/CLAUDE.md](.github/CLAUDE.md)** - Issues, PRs, labels, contributions

- When: Creating issues, pull requests, managing labels
- Covers: Issue templates, PR process, label system

### Documentation

→ **[docs/CLAUDE.md](docs/CLAUDE.md)** - 83 files across 11 folders

- When: Finding or creating documentation
- Covers: Documentation navigation, contribution guide

#### Documentation Subcategories

- **[docs/architecture/CLAUDE.md](docs/architecture/CLAUDE.md)** - ADRs, domain model, event chains
- **[docs/development/CLAUDE.md](docs/development/CLAUDE.md)** - Coding standards, workflows, patterns
- **[docs/security/CLAUDE.md](docs/security/CLAUDE.md)** - Threat model, OWASP, RLS

### Agent OS (Spec-Driven Development)

→ **[agent-os/](agent-os/)** - Spec-driven development infrastructure

- When: Loading module context, creating feature specs, following patterns
- Covers: DDD module profiles, standards, spec templates, Claude Code skills
- **Profiles:** `agent-os/profiles/` (8 modules + 5 layers)
- **Standards:** `agent-os/standards/` (extracted patterns)
- **Specs:** `agent-os/specs/` (machine-readable feature specs)
- **Skills:** `.claude/skills/` (implementation guides)

---

## Documentation Index

**Complete documentation:** [docs/INDEX.md](docs/INDEX.md) - Strategic foundation documentation

**Key Documents:**

- **Architecture:** [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md), [ADR-003](docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md), [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md), [event-chains-reference.md](docs/architecture/event-chains-reference.md)
- **Development:** [CODING_STANDARDS.md](docs/development/CODING_STANDARDS.md), [WORKFLOWS.md](docs/development/WORKFLOWS.md), [PATTERNS.md](docs/development/PATTERNS.md), [CLAUDE_CODE_GUIDE.md](docs/development/CLAUDE_CODE_GUIDE.md)
- **Product:** [FEATURE_BACKLOG.md](docs/product-strategy/FEATURE_BACKLOG.md), [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md), [PRODUCT_STRATEGY.md](docs/product-strategy/PRODUCT_STRATEGY.md)
- **Development Guides:** [Backend](docs/guides/BACKEND_DEVELOPMENT.md), [Frontend](docs/guides/FRONTEND_DEVELOPMENT.md), [Database](docs/guides/DATABASE_DEVELOPMENT.md), [Infrastructure](docs/guides/INFRASTRUCTURE_DEVELOPMENT.md)

---

## Working with Claude Code

**AI-Assisted Development Guide:** [docs/development/CLAUDE_CODE_GUIDE.md](docs/development/CLAUDE_CODE_GUIDE.md)

**Quick Reference:**

- **Simple tasks (1-2 files):** Direct implementation
- **Moderate tasks (3-5 files):** Explore → Implement
- **Complex tasks (5+ files):** Explore → Plan → Implement
- **Always discover patterns before implementing**
- **Always provide educational insights**

**Commit Format:**

```
<type>(<scope>): <summary> (#<issue>)

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

---

## Contributing

**Process:** [CONTRIBUTING.md](CONTRIBUTING.md)

**Templates:**

- Issues: `.github/ISSUE_TEMPLATE/` (Feature Request, Bug Report, Phase Deliverable, Research, Technical Debt)
- Pull Requests: `.github/PULL_REQUEST_TEMPLATE.md`

**Labels:** 60+ labels across Type, Phase, Service, Status, Priority, Domain, Effort, Special

---

## Context Hints for Claude Code

**When user says...**

- **"plan GraphQL API"** → Load [docs/guides/BACKEND_DEVELOPMENT.md](docs/guides/BACKEND_DEVELOPMENT.md) → GraphQL Input→Command pattern
- **"plan component architecture"** → Load [docs/guides/FRONTEND_DEVELOPMENT.md](docs/guides/FRONTEND_DEVELOPMENT.md) → Component architecture
- **"plan database schema"** → Load [docs/guides/DATABASE_DEVELOPMENT.md](docs/guides/DATABASE_DEVELOPMENT.md) → Schema design, RLS patterns
- **"plan infrastructure"** → Load [docs/guides/INFRASTRUCTURE_DEVELOPMENT.md](docs/guides/INFRASTRUCTURE_DEVELOPMENT.md) → Docker, K8s patterns
- **"review domain model"** → Load [docs/architecture/domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md)
- **"review event chains"** → Load [docs/architecture/event-chains-reference.md](docs/architecture/event-chains-reference.md)

---

**Last Updated:** 2026-02-09
**Version:** 7.0.0 (Active Development - Auth + Family Modules)
