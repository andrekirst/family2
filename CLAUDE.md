# Family Hub - Claude Code Guide

## Critical Context

**Family Hub** is a privacy-first, cloud-based family organization platform focused on intelligent **event chain automation** (automatic cross-domain workflows that no competitor offers).

**Tech Stack:** .NET Core 10 (C# 14, Hot Chocolate GraphQL) | Angular v21 (TypeScript, Tailwind) | PostgreSQL 16 (RLS) | RabbitMQ | Zitadel OAuth | Docker Compose→Kubernetes | Vogen 8.0+

**Phase:** Phase 0 - Foundation & Tooling (IN PROGRESS). OAuth integration completed. Next: Frontend OAuth, then Phase 1 Core MVP.

**Architecture:** Modular Monolith (Phase 1-4) → Microservices (Phase 5+). See [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md).

**Development:** Single developer + Claude Code AI (60-80% boilerplate). DDD with 8 modules. Event-driven (RabbitMQ). 10-14 months to MVP.

---

## Domain-Specific Guides

**Choose the right guide for your task:**

### Backend Development

→ **[src/api/CLAUDE.md](src/api/CLAUDE.md)** - .NET, C#, GraphQL, DDD patterns

- When: Implementing commands, queries, domain logic, migrations
- Covers: EF Core, Vogen, GraphQL Input→Command, Domain events, Testing

### Frontend Development

→ **[src/frontend/CLAUDE.md](src/frontend/CLAUDE.md)** - Angular, TypeScript, Playwright

- When: Building components, pages, E2E tests, OAuth integration
- Covers: Component architecture, Apollo GraphQL, Playwright, OAuth PKCE

### Database Work

→ **[database/CLAUDE.md](database/CLAUDE.md)** - PostgreSQL, EF Core migrations, RLS

- When: Creating migrations, schema changes, RLS policies
- Covers: Migration workflows, schema design, debugging

### Infrastructure & DevOps

→ **[infrastructure/CLAUDE.md](infrastructure/CLAUDE.md)** - Docker, K8s, CI/CD

- When: Docker Compose changes, deployment, observability
- Covers: Local setup, K8s deployment, CI/CD pipelines

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

---

## Quick Start

### Local Development Setup

```bash
# 1. Clone repository
git clone https://github.com/andrekirst/family2.git
cd family2

# 2. Start infrastructure (Docker Compose)
cd infrastructure/docker
docker-compose up -d

# 3. Start backend
cd ../../src/api
dotnet run --project FamilyHub.Api

# 4. Start frontend (new terminal)
cd ../frontend/family-hub-web
npm install && npm start
```

**Full Guide:** [docs/development/LOCAL_DEVELOPMENT_SETUP.md](docs/development/LOCAL_DEVELOPMENT_SETUP.md)

### Feature Implementation Workflow

```
1. User requests feature
2. Ask clarifying questions (AskUserQuestion)
3. Spawn feature-dev:code-explorer (find existing patterns)
4. Spawn feature-dev:code-architect (design following patterns)
5. Implement following EXACT patterns from subagents
6. Generate tests following existing test patterns
```

**Full Workflow:** [docs/development/CLAUDE_CODE_GUIDE.md](docs/development/CLAUDE_CODE_GUIDE.md)

---

## Documentation Index

**Complete documentation:** [docs/INDEX.md](docs/INDEX.md) - 83 files across 11 folders

**Key Documents:**

- **Architecture:** [ADR-001](docs/architecture/ADR-001-MODULAR-MONOLITH-FIRST.md), [ADR-003](docs/architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md), [domain-model-microservices-map.md](docs/architecture/domain-model-microservices-map.md)
- **Development:** [CODING_STANDARDS.md](docs/development/CODING_STANDARDS.md), [WORKFLOWS.md](docs/development/WORKFLOWS.md), [PATTERNS.md](docs/development/PATTERNS.md)
- **Product:** [FEATURE_BACKLOG.md](docs/product-strategy/FEATURE_BACKLOG.md), [implementation-roadmap.md](docs/product-strategy/implementation-roadmap.md)
- **Testing:** [TESTING_WITH_PLAYWRIGHT.md](docs/development/TESTING_WITH_PLAYWRIGHT.md)
- **Troubleshooting:** [DEBUGGING_GUIDE.md](docs/development/DEBUGGING_GUIDE.md)

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

- **"add GraphQL mutation"** → Load [src/api/CLAUDE.md](src/api/CLAUDE.md) → GraphQL Input→Command pattern
- **"create component"** → Load [src/frontend/CLAUDE.md](src/frontend/CLAUDE.md) → Component architecture
- **"create migration"** → Load [database/CLAUDE.md](database/CLAUDE.md) → EF Core migrations
- **"debug PostgreSQL"** → Load [database/CLAUDE.md](database/CLAUDE.md) → RLS troubleshooting
- **"write E2E test"** → Load [src/frontend/CLAUDE.md](src/frontend/CLAUDE.md) → Playwright patterns
- **"extract module"** → Load [docs/development/MODULE_EXTRACTION_QUICKSTART.md](docs/development/MODULE_EXTRACTION_QUICKSTART.md)

---

**Last Updated:** 2026-01-09
**Version:** 5.0.0 (Folder-specific CLAUDE.md refactoring)
