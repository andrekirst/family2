# Claude Code Guide - AI-Assisted Development

**Purpose:** Guide Claude Code AI to write high-quality Family Hub code with 80-90% correctness (vs 40-60% baseline).

**Target Audience:** Claude Code AI + developers working with Claude

---

## Core Philosophy

Family Hub development relies heavily on **Claude Code AI** for:

- 60-80% of boilerplate code generation
- Pattern discovery in existing codebase
- Architecture design following DDD principles
- Test generation following existing patterns

**Key Success Factor:** Claude must **discover existing patterns** before implementing new features. Never generate code blindly.

---

## Standard Implementation Workflow

### MANDATORY Process

```
1. User requests feature (moderate detail)
   ↓
2. Ask clarifying questions (AskUserQuestion tool)
   ↓
3. Spawn feature-dev:code-explorer (find existing patterns)
   ↓
4. Spawn feature-dev:code-architect (design following patterns)
   ↓
5. Implement following EXACT patterns from subagents
   ↓
6. Generate tests following existing test patterns
   ↓
7. Run tests and fix issues
```

**Goal:** Achieve 80-90% code correctness on first implementation.

**Reference:** [IMPLEMENTATION_WORKFLOW.md](IMPLEMENTATION_WORKFLOW.md) for detailed workflow.

---

## Subagent Decision Tree

Choose the right approach based on task complexity:

### Simple Tasks (1-2 files)

**Examples:** Add property to entity, simple validation, single mutation

**Approach:** **Direct implementation**

```
User request → Read existing files → Implement directly
```

**Tools:** Read, Write, Edit

### Moderate Tasks (3-5 files)

**Examples:** New GraphQL mutation with command, new value object, simple event

**Approach:** **Explore → Implement**

```
User request → Spawn feature-dev:code-explorer → Implement following patterns
```

**Subagents:**

- `feature-dev:code-explorer` - Find existing patterns (e.g., "How are GraphQL mutations structured?")

### Complex Tasks (5+ files)

**Examples:** New DDD module, event chain, complex business logic

**Approach:** **Explore → Plan → Implement**

```
User request → Spawn feature-dev:code-explorer → Spawn feature-dev:code-architect → Implement
```

**Subagents:**

- `feature-dev:code-explorer` - Pattern discovery
- `feature-dev:code-architect` - Design following discovered patterns

### Architectural Tasks

**Examples:** New bounded context, cross-cutting concern, major refactoring

**Approach:** **Explore → Plan → Review → Implement**

```
User request → Spawn Explore agents → Spawn Plan agent → Review with user → Implement
```

**Subagents:**

- Multiple `Explore` agents (parallel exploration)
- `Plan` agent (architecture design)
- User review (AskUserQuestion for critical decisions)

---

## Preferred Subagents & Tools

### High-Priority Subagents

Use these frequently for pattern discovery and design:

| Subagent | When to Use | Example Prompt |
|----------|-------------|----------------|
| **feature-dev:code-explorer** | Find existing patterns in codebase | "Explore how GraphQL mutations are structured. Find examples of Input→Command pattern with Vogen value objects." |
| **feature-dev:code-architect** | Design implementation following patterns | "Design a new CreateTaskCommand following the patterns discovered. Include GraphQL Input, MediatR Command, and validation." |
| **Explore** | General codebase navigation | "Explore the Auth module structure. Identify key entities, value objects, and domain events." |
| **Plan** | Architecture planning | "Plan the extraction of Calendar module from Auth. Include DbContext, migrations, and GraphQL schema." |

### Specialized Domain Agents

Use for domain-specific expertise:

- `frontend-developer` - Angular components, services
- `backend-developer` - .NET API, GraphQL
- `typescript-pro` - TypeScript patterns
- `angular-architect` - Angular architecture

### Tool Preferences

**Use MORE:**

- **Serena** - Symbol navigation (find_symbol, find_referencing_symbols, replace_symbol_body)
- **Context7** - Up-to-date library documentation (especially for Angular, Hot Chocolate, Vogen)
- **Sequential-thinking** - Complex architectural decisions
- **Task subagents** - Exploration, planning, specialized work

**Use LESS:**

- Extensive code comments (code should be self-documenting per CODING_STANDARDS.md)
- Direct Bash commands for file operations (use Read, Write, Edit tools instead)

---

## Pattern Discovery Process

**CRITICAL:** Always discover patterns BEFORE implementing.

### Step 1: Identify What Patterns Are Needed

**Example User Request:** "Add a CreateTask mutation"

**Patterns Needed:**

- GraphQL mutation structure
- Input DTO → Command mapping
- Vogen value object creation
- MediatR command handling
- FluentValidation validation
- Domain event publishing (if needed)

### Step 2: Spawn feature-dev:code-explorer

**Example Prompt:**

```
Explore how GraphQL mutations are structured in the codebase.

Find:
1. Example GraphQL mutation method
2. Input DTO definition (primitives)
3. MediatR Command definition (Vogen value objects)
4. How Input is mapped to Command
5. Command handler implementation
6. Validation patterns (FluentValidation)
7. Test examples (xUnit + FluentAssertions)

Focus on: Family creation mutation as reference (if exists), or any mutation in Auth module.
```

### Step 3: Spawn feature-dev:code-architect

**Example Prompt:**

```
Based on patterns discovered by code-explorer, design a CreateTask mutation.

Requirements:
- Input: { title: string, description: string, dueDate: string }
- Command: CreateTaskCommand(TaskId, TaskTitle, TaskDescription, DueDate)
- Value objects: TaskId, TaskTitle, TaskDescription (Vogen)
- Business rules: Title required, max 200 chars; Description optional, max 2000 chars
- Domain event: TaskCreatedEvent

Design:
1. GraphQL Input DTO
2. MediatR Command
3. Vogen value objects (with validation)
4. Command handler
5. Validator (FluentValidation)
6. Domain event
7. Test structure

Follow EXACT patterns from existing mutations.
```

### Step 4: Implement Following Patterns

Use the designs from code-architect as blueprints. Copy structure, naming conventions, and patterns exactly.

---

## Commit Format

**Conventional Commits** with Co-Authored-By:

```
<type>(<scope>): <summary> (#<issue>)

<optional body>

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
```

### Types

| Type | When to Use | Example |
|------|-------------|---------|
| **feat** | New feature or capability | `feat(task): add CreateTask mutation (#42)` |
| **fix** | Bug fix | `fix(auth): resolve token expiration issue (#38)` |
| **docs** | Documentation only | `docs: update CLAUDE_CODE_GUIDE with examples` |
| **style** | Code style changes (formatting, no logic change) | `style(api): format with dotnet format` |
| **refactor** | Code restructuring (no behavior change) | `refactor(family): extract FamilyName value object` |
| **test** | Add or fix tests | `test(auth): add integration tests for CreateFamily` |
| **chore** | Build, dependencies, tooling | `chore: update Vogen to 8.0.1` |

### Scope

Use module or component name:

- `auth` - Auth module
- `family` - Family module
- `task` - Task module
- `calendar` - Calendar module
- `frontend` - Frontend changes
- `infra` - Infrastructure changes

### Examples

```bash
# Good commits
feat(task): add CreateTask mutation with validation (#42)
fix(auth): resolve database connection timeout (#38)
refactor(family): extract FamilyName to SharedKernel (#45)
test(task): add E2E tests for task creation flow (#42)
docs(development): create CLAUDE_CODE_GUIDE
chore(deps): update Hot Chocolate to 14.2.0

# Bad commits (avoid)
feat: stuff  # Too vague
WIP auth  # Not conventional format
Fixed bug  # No scope or issue reference
```

### Co-Authored-By

**ALWAYS** include Co-Authored-By for Claude-generated commits:

```
Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
```

This tracks Claude's contribution and helps analyze AI-assisted development effectiveness.

---

## Educational Insights

Claude MUST provide **"Insight" boxes** explaining patterns, trade-offs, and design decisions.

### Format

```
★ Insight ─────────────────────────────────────
1. [Key insight about pattern or architecture]
2. [Trade-off or design decision explained]
3. [Project-specific pattern or convention]
─────────────────────────────────────────────────
```

### When to Provide Insights

**ALWAYS provide insights when:**

- Implementing a new feature
- Discovering interesting patterns
- Making architectural decisions
- Using DDD concepts (Aggregates, Value Objects, Domain Events)
- Explaining trade-offs (e.g., why GraphQL Input→Command separation)

### Insight Categories

#### 1. Architectural Patterns

**Example:**

```
★ Insight ─────────────────────────────────────
1. GraphQL Input→Command separation enables clean validation layers
2. Input DTOs use primitives for JSON deserialization
3. Commands use Vogen value objects for domain correctness
─────────────────────────────────────────────────
```

#### 2. DDD Concepts

**Example:**

```
★ Insight ─────────────────────────────────────
1. FamilyName is a Value Object (identity by value, immutable)
2. Family is an Aggregate Root (enforces invariants)
3. FamilyCreatedEvent triggers event chains (event-driven architecture)
─────────────────────────────────────────────────
```

#### 3. Project-Specific Patterns

**Example:**

```
★ Insight ─────────────────────────────────────
1. One DbContext per module enforces bounded context boundaries
2. PostgreSQL RLS policies implement multi-tenant isolation at DB level
3. Event chains coordinate cross-domain workflows automatically
─────────────────────────────────────────────────
```

#### 4. Trade-Offs & Design Decisions

**Example:**

```
★ Insight ─────────────────────────────────────
1. Vogen value objects add verbosity but eliminate primitive obsession
2. FluentValidation in Command handlers vs constructors: centralized validation
3. GraphQL vs REST: Schema-first, type-safe, reduces over-fetching
─────────────────────────────────────────────────
```

### Where to Place Insights

- **In conversation:** After implementing a pattern
- **NOT in code:** Keep code self-documenting (per CODING_STANDARDS.md)
- **In documentation:** When creating architectural docs

---

## Module Extraction Pattern

When extracting bounded contexts from Auth module, follow [ADR-005](docs/architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md).

**Quick Reference:**

1. **Domain Layer:** Entities, Value Objects, Domain Events
2. **Application Layer:** Commands, Handlers, Validators
3. **Persistence Layer:** DbContext, Migrations, Repositories
4. **Presentation Layer:** GraphQL Types, Mutations, Queries

**Example:**

```bash
# Extract Family module from Auth
Modules/FamilyHub.Modules.Family/
├── Domain/
│   ├── Entities/Family.cs
│   ├── ValueObjects/FamilyName.cs
│   └── Events/FamilyCreatedEvent.cs
├── Application/
│   ├── Commands/CreateFamilyCommand.cs
│   └── Handlers/CreateFamilyCommandHandler.cs
├── Persistence/
│   ├── FamilyDbContext.cs
│   └── Configurations/FamilyConfiguration.cs
└── Presentation/
    ├── GraphQL/FamilyMutations.cs
    └── DTOs/CreateFamilyInput.cs
```

**See:** [MODULE_EXTRACTION_QUICKSTART.md](MODULE_EXTRACTION_QUICKSTART.md) (when created)

---

## Testing Patterns

Claude MUST generate tests following existing patterns.

### Unit Tests (xUnit + FluentAssertions)

**Pattern:**

```csharp
[Theory, AutoNSubstituteData]
public async Task Handle_ValidCommand_CreatesFamily(
    [Frozen] Mock<IFamilyRepository> repositoryMock,
    CreateFamilyCommandHandler sut,
    CreateFamilyCommand command)
{
    // Arrange
    repositoryMock.Setup(r => r.AddAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.FamilyId.Should().NotBeEmpty();
    repositoryMock.Verify(r => r.AddAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

**Key Patterns:**

- `[Theory, AutoNSubstituteData]` for tests with dependencies
- `FluentAssertions` for ALL assertions (never xUnit Assert)
- Create Vogen value objects manually (`.From()` or constructor)
- `[Frozen]` for shared mocks

### E2E Tests (Playwright)

**Pattern:**

```typescript
test('should create family successfully', async ({ page, authFixture, graphqlClient }) => {
  // Arrange
  await authFixture.loginAs('testuser@example.com');

  // Act
  const response = await graphqlClient.mutate({
    mutation: CREATE_FAMILY_MUTATION,
    variables: { input: { name: 'Test Family' } }
  });

  // Assert
  expect(response.data.createFamily.familyId).toBeTruthy();
  expect(response.data.createFamily.name).toBe('Test Family');
});
```

**See:** [TESTING_WITH_PLAYWRIGHT.md](TESTING_WITH_PLAYWRIGHT.md) (when created)

---

## Common Pitfalls to Avoid

### 1. Implementing Without Pattern Discovery

**❌ Bad:**

```
User: "Add CreateTask mutation"
Claude: [Immediately generates code without exploring]
```

**✅ Good:**

```
User: "Add CreateTask mutation"
Claude: "Let me first explore existing mutation patterns..."
Claude: [Spawns feature-dev:code-explorer]
Claude: [Spawns feature-dev:code-architect based on patterns]
Claude: [Implements following discovered patterns]
```

### 2. Using Primitives Instead of Vogen

**❌ Bad:**

```csharp
public sealed record CreateFamilyCommand(string Name);  // Primitive!
```

**✅ Good:**

```csharp
public sealed record CreateFamilyCommand(FamilyName Name);  // Vogen value object
```

### 3. Skipping Educational Insights

**❌ Bad:**

```
[Generates code silently without explanation]
```

**✅ Good:**

```
★ Insight ─────────────────────────────────────
1. Separating GraphQL Input from MediatR Command enables clean validation layers
2. Vogen value objects enforce domain rules at the type level
3. FluentValidation provides centralized validation with clear error messages
─────────────────────────────────────────────────
```

### 4. Not Following Existing Patterns

**❌ Bad:**

```csharp
// Generates new pattern inconsistent with codebase
public class CreateTaskDto { ... }  // Nobody else uses "Dto" suffix
```

**✅ Good:**

```csharp
// Follows discovered pattern
public sealed record CreateTaskInput { ... }  // Matches existing "Input" suffix
```

### 5. Using xUnit Assert Instead of FluentAssertions

**❌ Bad:**

```csharp
Assert.Equal(expected, actual);
Assert.NotNull(result);
```

**✅ Good:**

```csharp
actual.Should().Be(expected);
result.Should().NotBeNull();
```

---

## Quick Reference Checklist

Before implementing a feature, Claude should:

- [ ] Ask clarifying questions if requirements are unclear
- [ ] Spawn `feature-dev:code-explorer` to find existing patterns
- [ ] Spawn `feature-dev:code-architect` to design following patterns
- [ ] Implement using EXACT patterns discovered
- [ ] Use Vogen value objects (never primitives in commands)
- [ ] Use FluentAssertions in tests (never xUnit Assert)
- [ ] Generate tests following existing test structure
- [ ] Provide educational insights explaining patterns
- [ ] Use conventional commit format with Co-Authored-By
- [ ] Reference issue number in commit message

---

## Related Documentation

- **Implementation Workflow:** [IMPLEMENTATION_WORKFLOW.md](IMPLEMENTATION_WORKFLOW.md)
- **Coding Standards:** [CODING_STANDARDS.md](CODING_STANDARDS.md)
- **Development Workflows:** [WORKFLOWS.md](WORKFLOWS.md)
- **DDD Patterns:** [PATTERNS.md](PATTERNS.md)
- **Module Extraction:** [ADR-005](docs/architecture/ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md)

---

**Last Updated:** 2026-01-09
**Version:** 1.0.0
**Purpose:** Guide Claude Code AI to achieve 80-90% code correctness through pattern discovery and systematic implementation.
