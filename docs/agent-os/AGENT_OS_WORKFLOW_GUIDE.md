# Agent OS Workflow Guide

**Purpose:** Step-by-step guide for implementing features using Agent OS with Claude Code in Family Hub.

**Version:** 1.0.0 (Created: 2026-01-25)

---

## Overview

Agent OS is a **spec-driven development framework** that enhances Claude Code's ability to implement features consistently by providing:

1. **Context Profiles** - Machine-readable module and layer context
2. **Standards** - Extracted patterns for consistent implementation
3. **Specs** - YAML specifications that define features before coding
4. **Skills** - Step-by-step implementation guides

This guide walks through the complete workflow using a real example: implementing the "User Profiles" feature.

---

## The Agent OS Workflow

```
┌─────────────────────────────────────────────────────────────────┐
│                     AGENT OS WORKFLOW                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. CREATE ISSUE          2. CREATE SPEC                        │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │ GitHub Issue │ ──────► │  YAML Spec   │                      │
│  │ #XXX         │         │  feature.yaml│                      │
│  └──────────────┘         └──────────────┘                      │
│                                  │                              │
│                                  ▼                              │
│  3. LOAD CONTEXT          4. IMPLEMENT                          │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │ Module       │ ──────► │ Use Skills   │                      │
│  │ Profile.yaml │         │ & Standards  │                      │
│  └──────────────┘         └──────────────┘                      │
│                                  │                              │
│                                  ▼                              │
│  5. VERIFY                 6. UPDATE SPEC                       │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │ Tests Pass   │ ──────► │ Mark Done    │                      │
│  │ Manual Check │         │ Close Issue  │                      │
│  └──────────────┘         └──────────────┘                      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Step 1: Create GitHub Issue

Every feature starts with a GitHub issue. This creates traceability and allows progress tracking.

### Issue Template Selection

Choose the appropriate template:

- **Feature Request** - New features (most common)
- **Bug Report** - Fixing defects
- **Technical Debt** - Refactoring

### Required Information

```yaml
Phase: Phase 1 - Core MVP
Service: Auth Service (or appropriate module)
User Story: |
  As a [persona]
  I want [capability]
  So that [benefit]
Acceptance Criteria:
  - [ ] Criterion 1
  - [ ] Criterion 2
```

### Example: User Profiles Issue

See issue #100 for the "User Profiles" feature implementation.

---

## Step 2: Create Feature Spec

The spec is the **contract** between you and Claude Code. It defines:

- What the feature does
- How it's structured (domain, API, UI)
- How it will be tested
- Progress tracking

### Create Spec File

```bash
# Copy template
cp agent-os/specs/templates/feature.spec.yaml \
   agent-os/specs/active/user-profiles.spec.yaml
```

### Spec Structure

```yaml
apiVersion: agent-os/v1
kind: FeatureSpec
metadata:
  name: 'user-profiles'
  module: 'auth'           # Which DDD module owns this
  issue: '#100'            # GitHub issue reference
  phase: 'phase-1'
  status: pending          # pending | in_progress | completed

spec:
  description: |
    Brief description of what this feature does and why.

  acceptance:
    - 'AC1: Users can view their profile'
    - 'AC2: Users can update profile information'

  domain:
    aggregates: []         # Entities/aggregate roots
    valueObjects: []       # Vogen value objects
    events: []             # Domain events

  api:
    mutations: []          # GraphQL mutations
    queries: []            # GraphQL queries

  ui:
    pages: []              # Angular routes/pages
    components: []         # Reusable components

  testing:
    unit: []               # Unit test scenarios
    integration: []        # Integration test scenarios
    e2e: []                # Playwright E2E flows

progress:
  percentage: 0
  lastUpdated: '2026-01-25'
  completedItems: []
```

### Spec as Living Documentation

The spec is updated as you implement:

- Mark acceptance criteria complete
- Add file locations as code is written
- Update progress percentage
- Track completed items

---

## Step 3: Load Module Context

Before implementing, Claude Code should load the relevant context.

### Profile Hierarchy

```
agent-os/profiles/
├── shared/base.yaml          # Common patterns (all projects)
├── layers/
│   ├── backend.yaml          # .NET, C#, GraphQL
│   ├── frontend.yaml         # Angular, TypeScript
│   ├── database.yaml         # PostgreSQL, EF Core
│   └── testing.yaml          # xUnit, Playwright
└── modules/
    ├── auth.yaml             # Authentication module
    ├── family.yaml           # Family management
    └── calendar.yaml         # Calendar & events
```

### What Profiles Contain

```yaml
# Example: agent-os/profiles/modules/auth.yaml
module:
  name: Auth
  namespace: FamilyHub.Modules.Auth
  schema: auth
  status: implemented

paths:
  backend: src/api/Modules/FamilyHub.Modules.Auth/**
  frontend: src/frontend/family-hub-web/src/app/auth/**
  tests:
    unit: tests/FamilyHub.Tests.Unit/Auth/**
    e2e: src/frontend/family-hub-web/e2e/auth/**

domain:
  entities: [User, Family]
  valueObjects: [UserId, Email, FamilyId]
  events: [UserCreatedEvent, FamilyCreatedEvent]

relatedDocs:
  - docs/authentication/OAUTH_INTEGRATION_GUIDE.md
```

### Loading Context in Claude Code

When implementing, tell Claude Code:

```
I'm working on issue #100 (User Profiles feature).
Load context from:
- agent-os/profiles/modules/auth.yaml
- agent-os/specs/active/user-profiles.spec.yaml
```

---

## Step 4: Implement Using Skills

Skills are step-by-step implementation guides located in `.claude/skills/`.

### Available Skills

| Skill | Purpose |
|-------|---------|
| `backend/graphql-mutation.md` | Create GraphQL mutations with Input→Command pattern |
| `backend/graphql-query.md` | Create GraphQL queries |
| `backend/value-object.md` | Create Vogen value objects |
| `frontend/angular-component.md` | Create Angular components |
| `database/ef-migration.md` | Create EF Core migrations |
| `testing/unit-test.md` | Create unit tests |
| `testing/playwright-test.md` | Create E2E tests |
| `workflows/feature-implementation.md` | Complete feature workflow |

### Invoking Skills

Tell Claude Code to use a skill:

```
Use skill: backend/graphql-mutation
- mutationName: UpdateUserProfile
- module: auth
- fields: [displayName: string, avatarUrl: string]
```

### Implementation Order

The `workflows/feature-implementation.md` skill defines the order:

1. **Domain Layer** - Value objects, entities, events, repository interfaces
2. **Application Layer** - Commands, queries, handlers, validators
3. **Persistence Layer** - EF Core config, repository implementation, migrations
4. **Presentation Layer** - GraphQL types, mutations, queries
5. **Frontend** - Components, GraphQL operations, routes
6. **Testing** - Unit tests, integration tests, E2E tests
7. **Verification** - Build, test, manual check

---

## Step 5: Reference Standards

Standards in `agent-os/standards/` provide detailed pattern documentation.

### Standard Categories

```
agent-os/standards/
├── backend/
│   ├── graphql-input-command.md    # Input→Command pattern
│   ├── domain-events.md            # Event publishing
│   └── vogen-value-objects.md      # Value object patterns
├── frontend/
│   ├── angular-components.md       # Component architecture
│   └── apollo-graphql.md           # Apollo client patterns
├── database/
│   ├── ef-core-migrations.md       # Migration workflow
│   └── rls-policies.md             # Row-Level Security
├── testing/
│   ├── unit-testing.md             # xUnit patterns
│   └── playwright-e2e.md           # E2E test patterns
└── architecture/
    ├── ddd-modules.md              # DDD module structure
    └── event-chains.md             # Event chain patterns
```

### Using Standards

Reference standards when implementing:

```
Following the standard: agent-os/standards/backend/graphql-input-command.md

1. Create Input type with [GraphQLName]
2. Create Command record
3. Implement handler using MediatR
4. Register in HotChocolate
```

---

## Step 6: Update Spec & Track Progress

As you implement, update the spec to track progress.

### Progress Tracking

```yaml
progress:
  percentage: 60
  lastUpdated: '2026-01-25'
  completedItems:
    - 'Domain value objects'
    - 'Domain aggregate'
    - 'Domain events'
    - 'GraphQL mutations'
    - 'Command handlers'
```

### File Location Tracking

Add locations as code is created:

```yaml
domain:
  aggregates:
    - name: 'UserProfile'
      location: 'src/api/Modules/FamilyHub.Modules.Auth/Domain/Aggregates/UserProfile.cs'
```

### Final Status

When complete:

```yaml
metadata:
  status: completed

progress:
  percentage: 100
  lastUpdated: '2026-01-25'
  completedItems:
    - 'All items...'
```

---

## Complete Example: User Profiles Feature

### 1. Issue Created

```
Issue #100: [Feature] User Profiles
Phase: Phase 1 - Core MVP
Service: Auth Service
```

### 2. Spec Created

```
agent-os/specs/active/user-profiles.spec.yaml
```

### 3. Context Loaded

```
Profile: agent-os/profiles/modules/auth.yaml
Standards: graphql-input-command.md, angular-components.md
```

### 4. Implementation Sequence

| Step | Action | Skill/Standard |
|------|--------|----------------|
| 4.1 | Create `DisplayName` value object | `backend/value-object.md` |
| 4.2 | Create `UserProfile` aggregate | `workflows/feature-implementation.md` |
| 4.3 | Create `UserProfileUpdatedEvent` | `standards/backend/domain-events.md` |
| 4.4 | Create `UpdateUserProfileCommand` | `backend/graphql-mutation.md` |
| 4.5 | Create EF Core configuration | `database/ef-migration.md` |
| 4.6 | Create migration | `database/ef-migration.md` |
| 4.7 | Create `ProfileComponent` | `frontend/angular-component.md` |
| 4.8 | Create unit tests | `testing/unit-test.md` |
| 4.9 | Create E2E tests | `testing/playwright-test.md` |

### 5. Verification

```bash
dotnet build
dotnet test
npm run build
npx playwright test
```

### 6. Commit & Close

```bash
git commit -m "feat(auth): implement user profiles (#100)"
```

---

## Best Practices

### DO

- ✅ Create spec before implementing
- ✅ Load module context first
- ✅ Follow skill steps in order
- ✅ Reference standards for patterns
- ✅ Update spec progress as you work
- ✅ Verify builds and tests before committing

### DON'T

- ❌ Skip the spec (leads to inconsistent implementation)
- ❌ Implement without loading context (may miss patterns)
- ❌ Ignore standards (creates technical debt)
- ❌ Forget to update spec progress
- ❌ Commit without running tests

---

## Troubleshooting

### "Claude Code isn't following patterns"

Ensure context is loaded:

```
Read and follow: agent-os/profiles/modules/{module}.yaml
```

### "Implementation doesn't match existing code"

Use code-explorer agent first:

```
Spawn feature-dev:code-explorer to find existing patterns
```

### "Not sure which skill to use"

Check the workflow skill:

```
Read: .claude/skills/workflows/feature-implementation.md
```

### "Spec is too complex"

Break into smaller features:

- Create multiple specs
- Link with dependencies in specs

---

## Related Documentation

- [Agent OS README](README.md) - Overview and structure
- [Profile Reference](PROFILE_REFERENCE.md) - Profile schema details
- [Skill Reference](SKILL_REFERENCE.md) - All available skills
- [Feature Backlog](../product-strategy/FEATURE_BACKLOG.md) - Planned features

---

**Last Updated:** 2026-01-25
**Issue:** #100
