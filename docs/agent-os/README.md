# Agent OS: Spec-Driven Development

> **Agent OS enables rapid, consistent feature development through machine-readable context profiles, extracted standards, and spec-driven workflows with Claude Code.**

## Overview

Agent OS is a spec-driven development infrastructure that accelerates feature development by providing Claude Code with comprehensive context about your codebase's architecture, patterns, and standards.

**Key Benefits:**

- **Rapid Feature Development:** Implement features 60-80% faster by leveraging pre-loaded context and established patterns
- **Consistent Patterns:** Ensure all features follow DDD principles and established architectural patterns
- **Comprehensive Context:** Claude Code loads module profiles, standards, and specs for informed decision-making
- **Machine-Readable:** All context is structured in YAML/JSON for reliable AI parsing

---

## Directory Structure

```
agent-os/
├── profiles/              # DDD module & layer context profiles
│   ├── modules/          # 8 module profiles (Auth, Calendar, Task, Shopping, Health, Meal Planning, Finance, Communication)
│   └── layers/           # 5 layer profiles (Domain, Application, Infrastructure, Presentation, SharedKernel)
├── standards/            # Extracted architectural patterns & standards
│   ├── domain-patterns/  # DDD patterns, value objects, aggregates
│   ├── application-patterns/ # Commands, queries, handlers
│   └── infrastructure-patterns/ # Repositories, database, messaging
└── specs/                # Machine-readable feature specifications
    ├── auth/            # Auth module feature specs
    ├── calendar/        # Calendar module feature specs
    └── ...              # Other module specs
```

---

## Module Profiles (`profiles/modules/`)

**8 DDD Bounded Context Profiles:**

Each module profile provides comprehensive context about a bounded context, including:

- **Domain Layer:** Aggregates, entities, value objects, domain events
- **Application Layer:** Commands, queries, handlers, validators
- **Infrastructure Layer:** Repositories, database schema, external integrations
- **Presentation Layer:** GraphQL mutations, queries, subscriptions
- **Dependencies:** Module dependencies and integration points

**Available Modules:**

1. **Auth Module** (`auth-module-profile.yaml`) - Authentication, authorization, user management
2. **Calendar Module** (`calendar-module-profile.yaml`) - Events, scheduling, reminders
3. **Task Module** (`task-module-profile.yaml`) - Tasks, assignments, completion tracking
4. **Shopping Module** (`shopping-module-profile.yaml`) - Shopping lists, items
5. **Health Module** (`health-module-profile.yaml`) - Appointments, prescriptions, medications
6. **Meal Planning Module** (`mealplanning-module-profile.yaml`) - Meal plans, recipes
7. **Finance Module** (`finance-module-profile.yaml`) - Budgets, expenses, bills
8. **Communication Module** (`communication-module-profile.yaml`) - Notifications, messages

---

## Layer Profiles (`profiles/layers/`)

**5 Layer Profiles:**

Each layer profile defines standards, patterns, and best practices for a specific architectural layer:

1. **Domain Layer** (`domain-layer-profile.yaml`)
   - DDD patterns (aggregates, entities, value objects, domain events)
   - Business rule enforcement
   - Domain-driven design principles

2. **Application Layer** (`application-layer-profile.yaml`)
   - CQRS pattern (commands, queries, handlers)
   - Input validation and mapping
   - Application services and orchestration

3. **Infrastructure Layer** (`infrastructure-layer-profile.yaml`)
   - Repository pattern
   - Database access (EF Core)
   - Message broker integration (RabbitMQ)
   - External service integrations

4. **Presentation Layer** (`presentation-layer-profile.yaml`)
   - GraphQL API design (mutations, queries, subscriptions)
   - Input-to-command mapping pattern
   - Error handling and response formatting

5. **SharedKernel** (`shared-kernel-profile.yaml`)
   - Cross-cutting concerns (timestamps, IDs, base classes)
   - Shared value objects
   - Common abstractions

---

## Standards (`standards/`)

**Extracted Architectural Patterns:**

The standards directory contains extracted patterns from the codebase, organized by category:

### Domain Patterns (`standards/domain-patterns/`)

- **Value Objects:** Pattern for creating strongly-typed value objects with Vogen
- **Aggregates:** Pattern for aggregate root design and consistency boundaries
- **Domain Events:** Pattern for domain event definition and publication
- **Entities:** Pattern for entity design with identity and lifecycle management

### Application Patterns (`standards/application-patterns/`)

- **Commands:** Pattern for command definition and validation
- **Queries:** Pattern for query definition and data projection
- **Handlers:** Pattern for command/query handler implementation
- **Validators:** Pattern for FluentValidation integration

### Infrastructure Patterns (`standards/infrastructure-patterns/`)

- **Repositories:** Pattern for repository implementation with EF Core
- **Database Schema:** Pattern for schema design with PostgreSQL RLS
- **Message Broker:** Pattern for RabbitMQ integration with Polly resilience
- **External Services:** Pattern for external API integration

---

## Specs (`specs/`)

**Machine-Readable Feature Specifications:**

Feature specs provide comprehensive, machine-readable specifications for implementing features. Each spec includes:

- **Feature Overview:** Name, description, user story, acceptance criteria
- **Domain Model:** Aggregates, entities, value objects, domain events
- **Application Layer:** Commands, queries, handlers, validation rules
- **Infrastructure:** Repository contracts, database schema, external integrations
- **Presentation:** GraphQL API (mutations, queries, subscriptions)
- **Event Chains:** Automated workflows triggered by this feature
- **Test Plan:** Unit tests, integration tests, E2E tests

**Example Spec Structure:**

```yaml
feature:
  name: "Family Creation"
  module: "Family"
  description: "Allow users to create family groups"

domain:
  aggregates:
    - Family
  value_objects:
    - FamilyId, FamilyName, UserId
  domain_events:
    - FamilyCreated

application:
  commands:
    - CreateFamilyCommand
  handlers:
    - CreateFamilyCommandHandler
  validators:
    - CreateFamilyCommandValidator

infrastructure:
  repositories:
    - IFamilyRepository
  database:
    schema: "family"
    tables:
      - families

presentation:
  graphql:
    mutations:
      - createFamily
```

---

## Usage with Claude Code

### 1. Loading Module Context

When implementing a feature, Claude Code can load the relevant module profile to understand the bounded context:

```bash
# Example: Loading Auth module context
claude code: "Implement user registration feature"
→ Claude loads agent-os/profiles/modules/auth-module-profile.yaml
→ Understands domain model, existing commands, patterns
→ Implements feature following established patterns
```

### 2. Following Standards

Claude Code references extracted standards to ensure consistency:

```bash
# Example: Creating a new value object
claude code: "Create EmailAddress value object"
→ Claude loads agent-os/standards/domain-patterns/value-objects.yaml
→ Follows Vogen pattern for value object creation
→ Implements validation, equality, conversion
```

### 3. Implementing from Specs

Feature specs provide complete implementation blueprints:

```bash
# Example: Implementing from spec
claude code: "Implement family creation feature"
→ Claude loads agent-os/specs/family/family-creation-spec.yaml
→ Implements domain, application, infrastructure, presentation layers
→ Generates tests from spec test plan
```

---

## Claude Code Skills

**Integration with `.claude/skills/`:**

Agent OS integrates with Claude Code skills for guided workflows:

- **`shape-spec`** - Generate feature specs from high-level requirements
- **`plan-product`** - Create product plans using module profiles
- **`index-standards`** - Extract standards from existing codebase
- **`discover-standards`** - Find patterns in codebase for standardization

**See:** `.claude/skills/` for complete skill documentation

---

## Benefits of Spec-Driven Development

### For Development Speed

- **60-80% faster implementation:** Pre-loaded context eliminates discovery phase
- **Consistent patterns:** No need to figure out how to structure code
- **Automated boilerplate:** Claude generates repetitive code following standards

### For Code Quality

- **DDD compliance:** All features follow domain-driven design principles
- **Architectural consistency:** Module profiles ensure adherence to architecture
- **Pattern reuse:** Extracted standards promote proven patterns

### For Maintainability

- **Searchable patterns:** Standards documented in machine-readable format
- **Clear boundaries:** Module profiles define bounded context boundaries
- **Event chain clarity:** Specs document cross-module interactions

---

## Contributing to Agent OS

### Adding New Module Profiles

When extracting a new module, create a module profile:

1. Copy an existing module profile as template
2. Document domain model (aggregates, entities, value objects, events)
3. Document application layer (commands, queries, handlers)
4. Document infrastructure (repositories, database schema)
5. Document presentation (GraphQL mutations, queries)

### Extracting New Standards

When discovering reusable patterns:

1. Identify pattern category (domain, application, infrastructure)
2. Document pattern structure, validation, examples
3. Add to appropriate standards directory
4. Update related module profiles

### Creating Feature Specs

For new features:

1. Use `shape-spec` skill to generate initial spec
2. Refine spec with domain experts
3. Save to appropriate module's specs directory
4. Reference in implementation

---

## Resources

- **Module Profiles:** `agent-os/profiles/modules/`
- **Layer Profiles:** `agent-os/profiles/layers/`
- **Standards:** `agent-os/standards/`
- **Specs:** `agent-os/specs/`
- **Skills:** `.claude/skills/`
- **Development Guides:** [docs/guides/](../docs/guides/)
- **Architecture ADRs:** [docs/architecture/](../docs/architecture/)

---

**Last Updated:** 2026-02-01
**Version:** 1.0.0 (Strategic Foundation)
