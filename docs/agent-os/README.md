# Agent OS Integration

**Purpose:** Spec-driven development framework for consistent AI-assisted development in Family Hub.

**Version:** 3.0 (Integration date: 2026-01-25)

---

## Overview

Agent OS enhances the existing CLAUDE.md documentation approach by adding:

1. **Context Profiles** - Machine-readable context for each DDD module and technology layer
2. **Standards Discovery** - Patterns extracted from CLAUDE.md into reusable standards
3. **Spec-Driven Workflow** - YAML specifications for features before implementation
4. **Claude Code Skills** - Step-by-step implementation guides

### Directory Structure

```
agent-os/
├── profiles/
│   ├── shared/base.yaml          # Common tech stack and patterns
│   ├── layers/                   # Technology layer profiles
│   │   ├── backend.yaml          # .NET, C#, GraphQL
│   │   ├── frontend.yaml         # Angular, TypeScript
│   │   ├── database.yaml         # PostgreSQL, EF Core
│   │   ├── testing.yaml          # xUnit, Playwright
│   │   └── infrastructure.yaml   # Docker, K8s
│   └── modules/                  # DDD module profiles
│       ├── auth.yaml             # Authentication (100% implemented)
│       ├── family.yaml           # Family management (partial)
│       ├── calendar.yaml         # Calendar & events (planned)
│       └── ...                   # 9 modules total
├── standards/
│   ├── index.yml                 # Standards catalog
│   ├── backend/                  # Backend standards
│   ├── frontend/                 # Frontend standards
│   ├── database/                 # Database standards
│   ├── testing/                  # Testing standards
│   └── architecture/             # Architecture standards
└── specs/
    ├── templates/                # Spec templates
    │   ├── feature.spec.yaml
    │   ├── mutation.spec.yaml
    │   ├── query.spec.yaml
    │   ├── component.spec.yaml
    │   └── event-chain.spec.yaml
    └── active/                   # Active feature specs

.claude/skills/
├── backend/                      # Backend skills
│   └── graphql-mutation.md
├── frontend/                     # Frontend skills
│   └── angular-component.md
├── database/                     # Database skills
│   └── ef-migration.md
├── testing/                      # Testing skills
│   └── playwright-test.md
├── workflows/                    # Workflow skills
│   └── feature-implementation.md
└── meta/
    └── skill-manifest.json
```

---

## Quick Start

### 1. Load Module Context

When working on a specific module, Claude Code can load:

```yaml
# agent-os/profiles/modules/auth.yaml
extends: ../shared/base.yaml

module:
  name: Auth
  namespace: FamilyHub.Modules.Auth
  schema: auth
  status: implemented

context:
  paths:
    - src/api/Modules/FamilyHub.Modules.Auth/**
    - src/frontend/family-hub-web/src/app/auth/**
```

### 2. Create Feature Spec

Before implementing a feature, create a spec:

```bash
# Copy template
cp agent-os/specs/templates/feature.spec.yaml agent-os/specs/active/my-feature.spec.yaml

# Fill in the spec details
```

### 3. Use Skills

Skills provide step-by-step implementation guides:

```
Invoke skill: backend/graphql-mutation
- mutationName: CreateFamily
- module: auth
- fields: [name: string]
```

---

## Related Documentation

- [Getting Started Guide](GETTING_STARTED.md)
- [Profile Reference](PROFILE_REFERENCE.md)
- [Skill Reference](SKILL_REFERENCE.md)
- [Evaluation Criteria](EVALUATION_CRITERIA.md)

---

## Integration with Existing CLAUDE.md

Agent OS works **alongside** the existing CLAUDE.md documentation:

| Asset | Purpose | Format |
|-------|---------|--------|
| CLAUDE.md | Human-readable development guide | Markdown |
| Agent OS Profiles | Machine-readable context | YAML |
| Agent OS Standards | Extracted patterns | Markdown |
| Claude Code Skills | Implementation guides | Markdown |
| Spec Files | Feature specifications | YAML |

**Enhancement, not replacement:** CLAUDE.md remains the primary source of truth. Agent OS adds structured context for AI tools.

---

**Last Updated:** 2026-01-25
**Issue:** #98
