# Agent OS - Getting Started

Quick start guide for using Agent OS in Family Hub development.

---

## Prerequisites

- Family Hub repository cloned
- Development environment set up (see [LOCAL_DEVELOPMENT_SETUP.md](../development/LOCAL_DEVELOPMENT_SETUP.md))
- Claude Code or compatible AI assistant

---

## Understanding the Structure

### 1. Profiles (Context Loading)

Profiles provide context for specific modules or technology layers.

**Base Profile** (`agent-os/profiles/shared/base.yaml`):

- Tech stack versions
- Common patterns
- Shared standards

**Layer Profiles** (`agent-os/profiles/layers/`):

- `backend.yaml` - .NET, C#, GraphQL patterns
- `frontend.yaml` - Angular, TypeScript patterns
- `database.yaml` - PostgreSQL, EF Core patterns
- `testing.yaml` - xUnit, Playwright patterns
- `infrastructure.yaml` - Docker, K8s patterns

**Module Profiles** (`agent-os/profiles/modules/`):

- One per DDD module (auth, family, calendar, etc.)
- Contains: namespace, schema, entities, events, mutations, queries
- Links to related documentation

### 2. Standards (Patterns)

Standards are extracted patterns from CLAUDE.md documentation.

**Categories:**

- `backend/` - GraphQL Input→Command, Vogen, Domain Events
- `frontend/` - Angular Components, Apollo GraphQL
- `database/` - EF Core Migrations, RLS Policies
- `testing/` - Unit Testing, E2E Testing
- `architecture/` - DDD Modules, Event Chains

**Example Standard:**

```markdown
# GraphQL Input → Command Pattern

## Pattern
1. Create Input DTO with primitive types
2. Create Vogen value objects
3. Create MediatR Command with value objects
4. Map Input → Command in mutation

## Example
[Code examples...]
```

### 3. Skills (Implementation Guides)

Skills are step-by-step implementation guides for common tasks.

**Available Skills:**

- `backend/graphql-mutation.md` - Create GraphQL mutation
- `frontend/angular-component.md` - Create standalone component
- `database/ef-migration.md` - Create EF Core migration
- `testing/playwright-test.md` - Create Playwright E2E test
- `workflows/feature-implementation.md` - Complete feature workflow

### 4. Specs (Feature Specifications)

Specs define features before implementation.

**Templates:**

- `feature.spec.yaml` - Complete feature specification
- `mutation.spec.yaml` - GraphQL mutation specification
- `query.spec.yaml` - GraphQL query specification
- `component.spec.yaml` - Angular component specification
- `event-chain.spec.yaml` - Event automation specification

---

## Common Workflows

### Implementing a New Feature

1. **Create Feature Spec**

   ```bash
   cp agent-os/specs/templates/feature.spec.yaml \
      agent-os/specs/active/my-feature.spec.yaml
   ```

2. **Fill in Spec Details**
   - Description and acceptance criteria
   - Domain entities and value objects
   - API mutations and queries
   - UI components
   - Test scenarios

3. **Load Module Context**
   - Reference the appropriate module profile
   - AI assistant loads relevant context

4. **Follow Skill Steps**
   - Use `workflows/feature-implementation.md` skill
   - Skill references relevant standards

5. **Update Spec Progress**
   - Mark completed items in spec
   - Update percentage complete

### Adding a GraphQL Mutation

1. Load module profile
2. Follow `backend/graphql-mutation.md` skill
3. Reference `backend/graphql-input-command.md` standard

### Creating an Angular Component

1. Load frontend layer profile
2. Follow `frontend/angular-component.md` skill
3. Reference `frontend/angular-components.md` standard

---

## Integration with Claude Code

When working with Claude Code:

1. **Context Loading:** Claude Code can read profiles and standards
2. **Skill Invocation:** Reference skills for step-by-step guidance
3. **Spec Tracking:** Update specs as implementation progresses

**Example Prompt:**

```
Load the auth module profile and implement a new GraphQL mutation
following the graphql-mutation skill.
```

---

## Rollback

If Agent OS causes issues:

```bash
# Rollback to main branch
git checkout main
git branch -D feat/agent-os-integration
```

The existing CLAUDE.md documentation remains fully functional.

---

**Next:** [Profile Reference](PROFILE_REFERENCE.md)
