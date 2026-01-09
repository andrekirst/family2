# Development Patterns Guide

**Purpose:** Guide to coding standards, DDD patterns, development workflows, and implementation processes in Family Hub.

**Key Resources:** CODING_STANDARDS.md, PATTERNS.md, WORKFLOWS.md, IMPLEMENTATION_WORKFLOW.md

---

## Quick Reference

### Core Development Documents

1. **[CODING_STANDARDS.md](CODING_STANDARDS.md)** - Comprehensive coding standards (C#, TypeScript, DDD, GraphQL, Testing)
2. **[PATTERNS.md](PATTERNS.md)** - Domain-Driven Design patterns and examples
3. **[WORKFLOWS.md](WORKFLOWS.md)** - Database migrations, value objects, testing, GraphQL
4. **[IMPLEMENTATION_WORKFLOW.md](IMPLEMENTATION_WORKFLOW.md)** - Standard feature implementation process
5. **[LOCAL_DEVELOPMENT_SETUP.md](LOCAL_DEVELOPMENT_SETUP.md)** - Complete local dev setup
6. **[TESTING_WITH_PLAYWRIGHT.md](TESTING_WITH_PLAYWRIGHT.md)** - E2E testing guide
7. **[DEBUGGING_GUIDE.md](DEBUGGING_GUIDE.md)** - Troubleshooting reference
8. **[MODULE_EXTRACTION_QUICKSTART.md](MODULE_EXTRACTION_QUICKSTART.md)** - Bounded context extraction
9. **[CLAUDE_CODE_GUIDE.md](CLAUDE_CODE_GUIDE.md)** - AI-assisted development workflow
10. **[HOOKS.md](HOOKS.md)** - Automatic code formatting and quality checks

---

## Critical Patterns (4)

### 1. Domain-Driven Design Patterns

**Core DDD Concepts:**

**Aggregates:**

```csharp
public sealed class Family : AggregateRoot<FamilyId>
{
    public FamilyName Name { get; private set; }
    private readonly List<UserId> _memberIds = new();
    public IReadOnlyList<UserId> MemberIds => _memberIds.AsReadOnly();

    // Factory method (only way to create)
    public static Family Create(FamilyName name)
    {
        var family = new Family
        {
            Id = FamilyId.New(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        family.RaiseDomainEvent(new FamilyCreatedEvent(family.Id, family.Name));
        return family;
    }

    // Business logic encapsulated
    public void AddMember(UserId userId)
    {
        if (_memberIds.Contains(userId))
            throw new DomainException("User already member of family");

        _memberIds.Add(userId);
        RaiseDomainEvent(new MemberAddedToFamilyEvent(Id, userId));
    }
}
```

**Value Objects (Vogen):**

```csharp
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Family name cannot be empty.");

        if (value.Length > 100)
            return Validation.Invalid("Family name cannot exceed 100 characters.");

        return Validation.Ok;
    }

    private static string NormalizeInput(string input) => input.Trim();
}
```

**Domain Events:**

```csharp
public sealed record FamilyCreatedEvent(
    FamilyId FamilyId,
    FamilyName FamilyName
) : IDomainEvent;

// Handler
public sealed class FamilyCreatedEventHandler
    : INotificationHandler<FamilyCreatedEvent>
{
    private readonly IRabbitMqPublisher _publisher;

    public async Task Handle(
        FamilyCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(notification, cancellationToken);
    }
}
```

**See:** [PATTERNS.md](PATTERNS.md) for comprehensive DDD patterns.

---

### 2. Testing Philosophy

**Test Pyramid:**

```
        E2E Tests (Playwright)
       /                      \
      Integration Tests        |
     /                         |
    Unit Tests                 |
   ───────────────────────────────
```

**Unit Tests (xUnit + FluentAssertions + AutoNSubstituteData):**

```csharp
[Theory, AutoNSubstituteData]
public async Task Handle_ValidCommand_CreatesFamily(
    [Frozen] Mock<IFamilyRepository> repositoryMock,
    CreateFamilyCommandHandler sut,
    FamilyName familyName)
{
    // Arrange
    var command = new CreateFamilyCommand(familyName);
    repositoryMock
        .Setup(r => r.AddAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.FamilyId.Should().NotBeEmpty();
    repositoryMock.Verify(
        r => r.AddAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()),
        Times.Once);
}
```

**E2E Tests (Playwright - API-First):**

```typescript
test('should create family via API and verify in UI', async ({ page }) => {
  // 1. Create via GraphQL API (fast, reliable)
  const response = await page.request.post('http://localhost:7000/graphql', {
    data: {
      query: `
        mutation CreateFamily($input: CreateFamilyInput!) {
          createFamily(input: $input) {
            familyId
            name
          }
        }
      `,
      variables: {
        input: { name: 'Test Family' }
      }
    }
  });

  const data = await response.json();
  expect(data.data.createFamily.familyId).toBeTruthy();

  // 2. Spot-check UI (optional, lightweight)
  await page.goto('/family');
  await expect(page.getByText('Test Family')).toBeVisible();
});
```

**Zero-Retry Policy:** All tests must pass reliably without retries. Fix flaky tests immediately.

**See:** [TESTING_WITH_PLAYWRIGHT.md](TESTING_WITH_PLAYWRIGHT.md) for comprehensive E2E guide.

---

### 3. Code Quality Standards

**C# Standards:**

- Use nullable reference types (`#nullable enable`)
- Prefer `record` over `class` for DTOs
- Always use Vogen for value objects
- FluentAssertions for all assertions
- XML comments for public APIs
- Follow .NET naming conventions

**TypeScript Standards:**

- Strict mode enabled (`"strict": true`)
- No explicit `any` (use `unknown` + type guards)
- Prefer `const` over `let`
- Use interfaces for data shapes
- ESLint + Prettier enforced via hooks

**GraphQL Standards:**

- Input DTOs use primitives (primitives for JSON deserialization)
- Commands use Vogen value objects (domain correctness)
- Document all queries, mutations, types
- PascalCase for types, camelCase for fields

**DDD Standards:**

- Aggregates own business logic
- Value objects immutable
- Domain events for cross-aggregate communication
- Repository interfaces in domain, implementations in infrastructure
- No domain logic in presentation layer

**See:** [CODING_STANDARDS.md](CODING_STANDARDS.md) for complete standards.

---

### 4. Implementation Workflow

**Standard Feature Implementation Process:**

1. **Understand Requirements**
   - Check [FEATURE_BACKLOG.md](../product-strategy/FEATURE_BACKLOG.md)
   - Review [wireframes.md](../ux-design/wireframes.md)
   - Identify affected module in [domain-model-microservices-map.md](../architecture/domain-model-microservices-map.md)

2. **Explore Codebase** (Claude Code: feature-dev:code-explorer)
   - Find existing patterns
   - Identify similar implementations
   - Understand module structure

3. **Design Implementation** (Claude Code: feature-dev:code-architect)
   - Follow discovered patterns EXACTLY
   - Design domain model (aggregates, value objects, events)
   - Plan database migrations
   - Design GraphQL schema

4. **Implement**
   - Domain layer first (aggregates, value objects)
   - Application layer (commands, queries, handlers)
   - Persistence layer (EF Core configs, migrations)
   - Presentation layer (GraphQL types, mutations)

5. **Test**
   - Unit tests (domain logic)
   - Integration tests (repository, database)
   - E2E tests (GraphQL API + UI spot checks)

6. **Document**
   - Update relevant docs
   - Add code comments where needed
   - Update CHANGELOG.md

**See:** [IMPLEMENTATION_WORKFLOW.md](IMPLEMENTATION_WORKFLOW.md) for detailed process.

---

## Development Workflows

### Database Migrations (EF Core)

**Create Migration:**

```bash
dotnet ef migrations add <MigrationName> \
  --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api \
  --output-dir Persistence/Migrations
```

**Apply Migration:**

```bash
# Development
dotnet ef database update --context AuthDbContext

# Production (in Program.cs)
await context.Database.MigrateAsync();
```

**See:** [WORKFLOWS.md](WORKFLOWS.md#database-migrations-with-ef-core)

---

### GraphQL Input→Command Pattern

**Separate Input DTOs (primitives) from Commands (Vogen):**

```csharp
// Input DTO (primitives)
public sealed record CreateFamilyInput
{
    [Required]
    public required string Name { get; init; }
}

// Command (Vogen)
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>;

// Mutation (mapping)
public async Task<CreateFamilyPayload> CreateFamily(
    CreateFamilyInput input,
    [Service] IMediator mediator)
{
    var command = new CreateFamilyCommand(
        FamilyName.From(input.Name)  // Primitive → Vogen
    );

    var result = await mediator.Send(command);
    return new CreateFamilyPayload(result);
}
```

**See:** [WORKFLOWS.md](WORKFLOWS.md#graphql-inputcommand-pattern) and [ADR-003](../architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)

---

### Automatic Code Formatting

**PostToolUse hooks** run automatically after file changes:

- **TypeScript:** Prettier + ESLint
- **C#:** dotnet format

**Configuration:** [HOOKS.md](HOOKS.md)

---

## Common Development Tasks

### Create New Module

1. Follow [MODULE_EXTRACTION_QUICKSTART.md](MODULE_EXTRACTION_QUICKSTART.md)
2. Create module structure (Domain, Application, Persistence, Presentation)
3. Add DbContext with schema
4. Create initial migration
5. Register services in Program.cs

### Add GraphQL Mutation

1. Create Input DTO (primitives)
2. Create Command (Vogen)
3. Create Command Handler
4. Create Mutation method
5. Add tests (unit, integration, E2E)

### Debug Issues

See [DEBUGGING_GUIDE.md](DEBUGGING_GUIDE.md) for:

- Build errors (C#, TypeScript, Docker)
- Runtime errors (exceptions, GraphQL)
- Database issues (migrations, RLS)
- RabbitMQ connectivity
- Performance profiling

---

## Educational Insights

**Development-Specific Examples:**

```
★ Insight ─────────────────────────────────────
1. DDD aggregates enforce business invariants at the root level
2. Value objects (Vogen) prevent primitive obsession and invalid states
3. Domain events enable loose coupling between bounded contexts
─────────────────────────────────────────────────
```

```
★ Insight ─────────────────────────────────────
1. GraphQL Input→Command pattern separates framework concerns from domain
2. FluentAssertions provides readable test assertions
3. API-first E2E testing is 10x faster than UI-only testing
─────────────────────────────────────────────────
```

```
★ Insight ─────────────────────────────────────
1. Zero-retry policy forces fixing flaky tests immediately
2. Automatic formatting via hooks ensures consistency
3. Claude Code subagents achieve 80-90% code correctness
─────────────────────────────────────────────────
```

---

## Related Documentation

- **Backend Guide:** [../../src/api/CLAUDE.md](../../src/api/CLAUDE.md) - Backend patterns
- **Frontend Guide:** [../../src/frontend/CLAUDE.md](../../src/frontend/CLAUDE.md) - Frontend patterns
- **Database Guide:** [../../database/CLAUDE.md](../../database/CLAUDE.md) - Database patterns
- **Architecture Guide:** [../architecture/CLAUDE.md](../architecture/CLAUDE.md) - ADRs and domain model

---

**Last Updated:** 2026-01-09
**Derived from:** Root CLAUDE.md v5.0.0
**Canonical Sources:**

- CODING_STANDARDS.md (Code quality requirements)
- PATTERNS.md (DDD patterns and examples)
- WORKFLOWS.md (Database, testing, GraphQL workflows)
- IMPLEMENTATION_WORKFLOW.md (Feature development process)
- TESTING_WITH_PLAYWRIGHT.md (E2E testing patterns)

**Sync Checklist:**

- [ ] DDD examples match PATTERNS.md
- [ ] Testing patterns match WORKFLOWS.md
- [ ] Code standards summary aligns with CODING_STANDARDS.md
- [ ] GraphQL pattern matches ADR-003
- [ ] Module extraction reference accurate
