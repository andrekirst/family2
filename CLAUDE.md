# Family Hub - Claude Code Guide

> **⚠️ PROJECT RESTART (February 2026):** This project has undergone an architectural restart. All implementation code has been removed. This guide now references a strategic foundation template. See README.md for full context. Previous implementation preserved in Git tag `v0.1-phase0-archive`.

## Critical Context

**Family Hub** is a privacy-first family organization platform concept focused on intelligent **event chain automation** (automatic cross-domain workflows that no competitor offers).

**Current State:** Strategic foundation template with comprehensive documentation, domain models, and development guides. No runnable code.

**Tech Stack (Planned):** .NET Core (C#, Hot Chocolate GraphQL) | Angular (TypeScript, Tailwind) | PostgreSQL (RLS) | OAuth 2.0 / OIDC | Docker → Kubernetes

**Phase:** Phase 0 - Restart & Redesign. Architecture being reconsidered. Implementation timeline TBD.

**Architecture Strategy:** Modular Monolith First → Eventual Microservices. See [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md).

**Development:** Strategic foundation created with Claude Code AI assistance. Domain modeling, architecture, and comprehensive documentation.

---

## Domain-Specific Guides

**Choose the right guide for your task:**

### Backend Development

→ **[docs/guides/BACKEND_DEVELOPMENT.md](docs/guides/BACKEND_DEVELOPMENT.md)** - .NET, C#, GraphQL, DDD patterns

- When: Planning backend architecture, domain logic, GraphQL APIs
- Covers: EF Core, Vogen, GraphQL Input→Command, Domain events, Testing patterns

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

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
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

**Last Updated:** 2026-02-01
**Version:** 6.0.0 (Strategic Foundation - Post Restart)
