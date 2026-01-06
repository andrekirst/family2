# Development Workflows

**Purpose:** Detailed patterns for common development tasks. Load this document when implementing features that require specific workflow knowledge.

**When to reference:** Database migrations, value objects, testing, GraphQL integration, E2E tests.

---

## Database Migrations with EF Core

**CRITICAL:** Use EF Core Code-First migrations for ALL schema changes (never custom SQL scripts).

### Pattern

One DbContext per module (Auth, Calendar, etc.), each targeting its own PostgreSQL schema. Fluent API configurations in `IEntityTypeConfiguration<T>` classes, PostgreSQL-specific features (RLS, triggers) via `migrationBuilder.Sql()`.

### Commands

```bash
# Create migration
dotnet ef migrations add <Name> --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api

# Apply migration (development)
dotnet ef database update --context AuthDbContext

# Production (in Program.cs)
await context.Database.MigrateAsync();
```

### Vogen Integration

```csharp
// In IEntityTypeConfiguration<User>
builder.Property(u => u.Id)
    .HasConversion(new UserId.EfCoreValueConverter())
    .IsRequired();
```

### Reference

Original SQL design scripts in `/database/docs/reference/sql-design/` (informational only, NOT executed).

---

## Value Objects with Vogen

**CRITICAL:** Use Vogen for ALL value objects (never manual base classes).

### Pattern

```csharp
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct UserId
{
    // Vogen source generator auto-generates:
    // - Equality operators
    // - EF Core converter
    // - JSON serialization
    // - Validation
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email cannot be empty");

        if (!value.Contains('@'))
            return Validation.Invalid("Invalid email format");

        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
        => input?.Trim().ToLowerInvariant() ?? string.Empty;
}
```

### Creation

```csharp
// New GUID
UserId userId = UserId.New();

// With validation
Email email = Email.From("user@example.com"); // Throws if invalid
Email.TryFrom("invalid", out var result);     // Safe creation

// In tests (manual creation)
var testEmail = Email.From("test@example.com");
```

### EF Core Configuration

```csharp
builder.Property(u => u.Email)
    .HasConversion(new Email.EfCoreValueConverter())
    .HasMaxLength(255);
```

### Examples

See `/src/api/FamilyHub.SharedKernel/Domain/ValueObjects/` for Email, UserId, FamilyId patterns.

---

## GraphQL Input/Command Pattern

**CRITICAL:** Maintain separate GraphQL Input DTOs (primitive types) that map to MediatR Commands (Vogen value objects).

### Why

HotChocolate cannot natively deserialize Vogen value objects from JSON. Input → Command mapping provides explicit conversion point and framework compatibility.

### Pattern

```csharp
// GraphQL Input DTO (primitives)
public record CreateFamilyInput
{
    public string Name { get; init; } = string.Empty;
}

// MediatR Command (Vogen value objects)
public record CreateFamilyCommand(FamilyName Name) : IRequest<CreateFamilyPayload>;

// Mutation method
public async Task<CreateFamilyPayload> CreateFamilyAsync(
    CreateFamilyInput input,
    [Service] IMediator mediator)
{
    var command = new CreateFamilyCommand(
        FamilyName.From(input.Name) // Explicit conversion
    );

    return await mediator.Send(command);
}
```

### Decision Rationale

[ADR-003](../architecture/ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md) - Attempted command-as-input pattern, failed due to Vogen incompatibility.

---

## Testing Patterns

### FluentAssertions

**CRITICAL:** Use FluentAssertions for ALL assertions (never xUnit `Assert.*`).

```csharp
// Basic assertions
actual.Should().Be(expected);
result.Should().NotBeNull();
collection.Should().HaveCount(3);

// Async assertions
await act.Should().ThrowAsync<InvalidOperationException>();
await task.Should().CompleteWithinAsync(TimeSpan.FromSeconds(5));

// Object assertions
user.Should().BeEquivalentTo(expected, options => options
    .Excluding(u => u.Id)
    .Excluding(u => u.CreatedAt));
```

Docs: <https://fluentassertions.com/>

### AutoFixture with NSubstitute

**CRITICAL:** Use `[Theory, AutoNSubstituteData]` for ALL tests with dependencies.

```csharp
[Theory, AutoNSubstituteData]
public async Task CreateFamily_Success(
    // Dependencies auto-injected by AutoFixture
    IFamilyRepository repository,
    IMediator mediator,
    CreateFamilyCommand command)
{
    // Arrange - configure only what matters
    repository.ExistsByNameAsync(Arg.Any<FamilyName>())
        .Returns(false);

    // Act
    var handler = new CreateFamilyCommandHandler(repository, mediator);
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    await repository.Received(1).AddAsync(Arg.Any<Family>());
}
```

**Vogen Policy:** Always create Vogen value objects manually in tests (improves clarity):

```csharp
// GOOD
var familyName = FamilyName.From("Test Family");
var userId = UserId.New();

// BAD (don't let AutoFixture generate Vogen types)
// AutoFixture can't properly generate valid Vogen instances
```

Custom attribute location: `/src/api/tests/FamilyHub.Tests.Unit/AutoNSubstituteDataAttribute.cs`

---

## E2E Testing with Playwright

**CRITICAL:** Use Playwright for ALL E2E tests (migrated from Cypress January 2026).

### Test Structure

```
e2e/
├── fixtures/          # Reusable test fixtures (auth, graphql, rabbitmq)
├── support/           # Helper utilities (constants, vogen-mirrors, api-helpers)
├── tests/             # Test files (.spec.ts)
├── global-setup.ts    # Testcontainers lifecycle
└── global-teardown.ts # Cleanup
```

### Key Patterns

#### 1. Fixtures (Dependency Injection)

```typescript
test("should create family", async ({
  authenticatedPage,
  interceptGraphQL,
}) => {
  await interceptGraphQL("GetCurrentFamily", { data: { family: null } });
  await authenticatedPage.goto("/family/create");
  // Test uses OAuth tokens automatically
});
```

#### 2. Vogen TypeScript Mirrors

```typescript
// Mirror C# Vogen validation in TypeScript
const familyName = FamilyName.from("Smith Family"); // Throws if invalid
const userId = UserId.new(); // Generates new GUID
```

#### 3. API-First Event Chain Testing

```typescript
test("doctor appointment event chain", async ({ rabbitmq }) => {
  // 1. Create via GraphQL API (10x faster than UI)
  const result = await client.mutate(CREATE_APPOINTMENT_MUTATION, variables);

  // 2. Verify RabbitMQ event published
  const event = await rabbitmq.waitForMessage(
    (msg) => msg.eventType === "HealthAppointmentScheduled",
    5000
  );

  // 3. Query backend to verify entities created
  const calendarEvents = await client.query(GET_CALENDAR_EVENTS);

  // 4. Spot-check UI (optional)
  await page.goto("/calendar");
  await expect(page.getByText("Doctor: Dr. Smith")).toBeVisible();
});
```

### Running Tests

```bash
# Local development
npm run e2e              # UI mode (interactive debugging)
npm run e2e:headless     # Headless mode
npm run e2e:chromium     # Single browser
npm run e2e:debug        # Debug mode with breakpoints

# CI/CD
npx playwright test      # Runs all tests on 3 browsers
```

### Test Organization

- **family-creation.spec.ts**: Main E2E tests (happy path, validation, errors)
- **accessibility.spec.ts**: WCAG 2.1 AA compliance (axe-core)
- **cross-browser.spec.ts**: Smoke tests (Chromium, Firefox, WebKit)
- **event-chains.spec.ts**: Event chain templates (SKIPPED until Phase 2)

### Zero-Retry Policy

`retries: 0` forces fixing flaky tests immediately (never mask issues with retries).

### Reference

[ADR-004-PLAYWRIGHT-MIGRATION.md](../architecture/ADR-004-PLAYWRIGHT-MIGRATION.md) - Migration rationale, patterns, metrics.

---

## Git Workflow

### Commit Format

```
<type>(<scope>): <summary> (#<issue>)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
```

**Types:** feat, fix, docs, style, refactor, test, chore

**Examples:**

```
feat(auth): add OAuth 2.0 flow (#42)
fix(calendar): resolve timezone offset bug (#58)
test(family): add creation validation tests (#61)
```

### Branching

See [git-workflow.md](git-workflow.md) for branching strategy and automation.

---

## Automatic Code Formatting

### Hook Configuration

Family Hub uses Claude Code PostToolUse hooks to automatically format code after AI-assisted edits.

**Configured formatters:**

- **TypeScript/JavaScript:** Prettier + ESLint (frontend)
- **C# files:** dotnet format (backend)
- **Markdown files:** markdownlint-cli2 (documentation) ← NEW
- **JSON/YAML files:** Prettier (configuration) ← NEW

**Configuration:** `.claude/settings.json` (committed to git)

**How it works:**

1. Claude edits a file using Edit or Write tool
2. Hook detects file extension
3. Appropriate formatter runs automatically
4. Changes appear in next file read/git diff

**Manual formatting:**

Frontend:

```bash
cd src/frontend/family-hub-web
npm run lint -- --fix
npm run lint:md:fix  # Markdown
npx prettier --write "src/**/*.{ts,js,html,css,scss}"
```

Backend:

```bash
cd src/api
dotnet format
```

Documentation:

```bash
npx markdownlint-cli2 --fix "**/*.md"  # All markdown files
npx prettier --write "**/*.{json,yml,yaml}"  # All config files
```

**Token Savings:**

- Markdownlint: **40% reduction** for documentation-heavy sessions (191 markdown files, 280K words)
- JSON/YAML formatting: Consistent configuration files reduce context noise

**Troubleshooting:**

If hooks fail (rare):

1. Check hook execution: Look for errors in Claude Code output
2. Disable temporarily: Add to `.claude/settings.local.json`:

   ```json
   {
     "hooks": {
       "PostToolUse": []
     }
   }
   ```

3. Re-enable: Remove override from `.local.json`

**Full guide:** [HOOKS.md](HOOKS.md) - Comprehensive hook documentation

---

**Last updated:** 2026-01-06
**Version:** 1.0.0
